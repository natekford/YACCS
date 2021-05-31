using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class Command : EntityBase, ICommand
	{
		public abstract Type? ContextType { get; }
		public IList<IReadOnlyList<string>> Names { get; set; }
		public IList<IParameter> Parameters { get; set; }
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		private string DebuggerDisplay => $"Name = {Names[0]}, Parameter Count = {Parameters.Count}";

		protected Command(MethodInfo method) : base(method)
		{
			Names = new List<IReadOnlyList<string>>();
			Parameters = method.GetParameters().Select(x => new Parameter(x)).ToList<IParameter>();
		}

		public abstract IEnumerable<IImmutableCommand> ToImmutable();

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		protected abstract class ImmutableCommand : IImmutableCommand
		{
			private readonly Lazy<Func<Task, object>> _TaskResultDelegate;

			public IReadOnlyList<object> Attributes { get; }
			public Type? ContextType { get; }
			public int MaxLength { get; }
			public int MinLength { get; }
			public IReadOnlyList<IReadOnlyList<string>> Names { get; }
			public IReadOnlyList<IImmutableParameter> Parameters { get; }
			public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
			public string PrimaryId { get; }
			public int Priority { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
			protected Type ReturnType { get; }
			private string DebuggerDisplay => $"Name = {Names?.FirstOrDefault()?.ToString() ?? "NULL"}, Parameter Count = {Parameters.Count}";

			protected ImmutableCommand(Command mutable, Type returnType)
			{
				ReturnType = returnType;
				ContextType = mutable.ContextType;
				_TaskResultDelegate = ReflectionUtils.CreateDelegate(CreateTaskResultDelegate,
					"task result delegate");

				{
					var names = ImmutableArray.CreateBuilder<IReadOnlyList<string>>(mutable.Names.Count);
					foreach (var name in mutable.Names)
					{
						names.Add(new ImmutableName(name));
					}
					Names = names.MoveToImmutable();
				}

				{
					var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(mutable.Parameters.Count);
					for (var i = 0; i < mutable.Parameters.Count; ++i)
					{
						var immutable = mutable.Parameters[i].ToImmutable(this);
						parameters.Add(immutable);

						// Remainder will always be the last parameter
						if (!immutable.Length.HasValue)
						{
							if (i != mutable.Parameters.Count - 1)
							{
								throw new ArgumentException("Cannot have multiple remainders and/or remainder must be the final parameter.");
							}

							MaxLength = int.MaxValue;
							break;
						}
						if (!immutable.HasDefaultValue)
						{
							MinLength += immutable.Length.Value;
						}
						MaxLength += immutable.Length.Value;
					}
					Parameters = parameters.MoveToImmutable();
				}

				{
					var attributes = ImmutableArray.CreateBuilder<object>(mutable.Attributes.Count);
					// Use ConcurrentDictionary because it has GetOrAdd by default, not threading reasons
					var preconditions = new ConcurrentDictionary<string, List<IPrecondition>>();
					var p = 0;
					foreach (var attribute in mutable.Attributes)
					{
						attributes.Add(attribute);
						switch (attribute)
						{
							case IPrecondition precondition:
								if (precondition.Groups.Count == 0)
								{
									preconditions.GetOrAdd("", _ => new List<IPrecondition>())
										.Add(precondition);
								}
								else
								{
									foreach (var group in precondition.Groups)
									{
										preconditions.GetOrAdd(group, _ => new List<IPrecondition>())
											.Add(precondition);
									}
								}
								break;

							case IPriorityAttribute priority:
								Priority = priority.ThrowIfDuplicate(x => x.Priority, ref p);
								break;

							case IIdAttribute id:
								PrimaryId ??= id.Id;
								break;
						}
					}
					Attributes = attributes.MoveToImmutable();
					Preconditions = preconditions.ToImmutableDictionary(
						x => x.Key,
						x => (IReadOnlyList<IPrecondition>)x.Value.ToImmutableArray()
					)!;
				}

				PrimaryId ??= Guid.NewGuid().ToString();
			}

			public abstract Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args);

			protected async Task<ExecutionResult> ConvertValueAsync(IContext context, object? value)
			{
				// Void method. No value to return, we're done
				if (ReturnType == typeof(void))
				{
					return new ExecutionResult(this, context, SuccessResult.Instance.Sync);
				}

				// We're given a task
				if (value is Task task)
				{
					// Let's await it to actually complete it
					await task.ConfigureAwait(false);

					// Not generic? No value to return, we're done
					if (!ReturnType.IsGenericType)
					{
						return new ExecutionResult(this, context, SuccessResult.Instance.Sync);
					}

					// It has a value? Ok, let's get it
					value = _TaskResultDelegate.Value.Invoke(task);
				}

				// We're given a result, we can just return that
				if (value is IResult result)
				{
					return new ExecutionResult(this, context, result);
				}

				// What do I do with random values?
				return new ExecutionResult(this, context, new ValueResult(value));
			}

			private Func<Task, object> CreateTaskResultDelegate()
			{
				/*
				 *	(Task Task) =>
				 *	{
				 *		return ((Task<T>)Task).Result;
				 *	}
				 */

				var instanceExpr = Expression.Parameter(typeof(Task), "Task");
				var instanceCastExpr = Expression.Convert(instanceExpr, ReturnType);
				var propertyExpr = Expression.Property(instanceCastExpr, nameof(Task<object>.Result));
				var propertyCastExpr = Expression.Convert(propertyExpr, typeof(object));

				var lambda = Expression.Lambda<Func<Task, object>>(propertyCastExpr, instanceExpr);
				return lambda.Compile();
			}
		}
	}
}