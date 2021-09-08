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
using YACCS.Commands.Building;
using YACCS.Commands.Linq;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class Command : EntityBase, ICommand
	{
		/// <inheritdoc />
		public Type ContextType { get; protected set; }
		/// <inheritdoc />
		public IList<IReadOnlyList<string>> Names { get; set; }
		public IReadOnlyList<IParameter> Parameters { get; protected set; }
		public IImmutableCommand? Source { get; protected set; }
		IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
		IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		protected Command(MethodInfo method, Type contextType, IImmutableCommand? source)
			: base(method)
		{
			Source = source;
			ContextType = contextType;
			Names = new List<IReadOnlyList<string>>();
			Parameters = method.GetParameters().Select(x => new Parameter(x)).ToList<IParameter>();
		}

		/// <inheritdoc />
		public virtual bool IsValidContext(Type type)
		{
			return ContextType.IsAssignableFrom(type) && Attributes
				.OfType<IContextConstraint>()
				.All(x => x.DoesTypeSatisfy(type));
		}

		/// <inheritdoc />
		public abstract IImmutableCommand ToImmutable();

		/// <inheritdoc />
		public virtual async IAsyncEnumerable<IImmutableCommand> ToMultipleImmutableAsync(
			IServiceProvider services)
		{
			var immutable = ToImmutable();
			yield return immutable;

			foreach (var generator in this.Get<ICommandGeneratorAttribute>())
			{
				foreach (var generated in await generator.GenerateCommandsAsync(services, immutable).ConfigureAwait(false))
				{
					yield return generated;
				}
			}
		}

		[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
		protected abstract class ImmutableCommand : IImmutableCommand
		{
			private readonly Lazy<Func<Task, object>> _TaskResult;
			private readonly ConcurrentDictionary<Type, bool> _ValidContexts = new();

			public IReadOnlyList<object> Attributes { get; }
			public Type ContextType { get; }
			public bool IsHidden { get; }
			public int MaxLength { get; }
			public int MinLength { get; }
			public IReadOnlyList<IReadOnlyList<string>> Names { get; }
			public IReadOnlyList<IImmutableParameter> Parameters { get; }
			public IReadOnlyDictionary<string, IReadOnlyList<IPrecondition>> Preconditions { get; }
			public string PrimaryId { get; }
			public int Priority { get; }
			public IImmutableCommand? Source { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			IEnumerable<IReadOnlyList<string>> IQueryableCommand.Names => Names;
			IReadOnlyList<IQueryableParameter> IQueryableCommand.Parameters => Parameters;
			protected Type ReturnType { get; }
			private string DebuggerDisplay => this.FormatForDebuggerDisplay();

			protected ImmutableCommand(Command mutable, Type returnType)
			{
				ReturnType = returnType;
				ContextType = mutable.ContextType;
				Source = mutable.Source;
				_TaskResult = new(() => ReflectionUtils.CreateDelegate(TaskResult, "task result"));

				var names = ImmutableArray.CreateBuilder<IReadOnlyList<string>>(mutable.Names.Count);
				foreach (var name in mutable.Names)
				{
					names.Add(new ImmutableName(name));
				}
				Names = names.MoveToImmutable();

				var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(mutable.Parameters.Count);
				for (var i = 0; i < mutable.Parameters.Count; ++i)
				{
					var immutable = mutable.Parameters[i].ToImmutable();
					parameters.Add(immutable);

					// Remainder will always be the last parameter
					if (!immutable.Length.HasValue)
					{
						if (i == mutable.Parameters.Count - 1)
						{
							MaxLength = int.MaxValue;
							break;
						}

						var orig = immutable.OriginalParameterName;
						var name = Names?.FirstOrDefault();
						throw new InvalidOperationException($"'{orig}' from '{name}' " +
							"must be the final parameter because it is a remainder.");
					}
					if (!immutable.HasDefaultValue)
					{
						MinLength += immutable.Length.Value;
					}
					MaxLength += immutable.Length.Value;
				}
				Parameters = parameters.MoveToImmutable();

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
								preconditions
									.GetOrAdd(string.Empty, _ => new())
									.Add(precondition);
							}
							else
							{
								foreach (var group in precondition.Groups)
								{
									preconditions
										.GetOrAdd(group, _ => new())
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

						case IHiddenAttribute:
							IsHidden = true;
							break;
					}
				}
				Attributes = attributes.MoveToImmutable();
				Preconditions = preconditions.ToImmutableDictionary(
					x => x.Key,
					x => (IReadOnlyList<IPrecondition>)x.Value.ToImmutableArray()
				);

				PrimaryId ??= Guid.NewGuid().ToString();
			}

			public abstract ValueTask<IResult> ExecuteAsync(IContext context, object?[] args);

			public virtual bool IsValidContext(Type type)
			{
				return _ValidContexts.GetOrAdd(type, static (type, args) =>
				{
					return args.ContextType.IsAssignableFrom(type) && args.Attributes
						.OfType<IContextConstraint>()
						.All(x => x.DoesTypeSatisfy(type));
				}, (ContextType, Attributes));
			}

			protected virtual ValueTask<IResult> ConvertValueAsync(object? value)
			{
				// Void method. No value to return, we're done
				if (ReturnType == typeof(void))
				{
					return new(SuccessResult.Instance);
				}

				// We're given a result, we can just return that
				if (value is IResult result)
				{
					return new(result);
				}

				// We're given a task
				if (value is Task task)
				{
					return new(ConvertValueAsync(task));
				}

				// Essentially box random values
				return new(new ValueResult(value));
			}

			protected virtual async Task<IResult> ConvertValueAsync(Task task)
			{
				// Let's await it to actually complete it
				await task.ConfigureAwait(false);

				// Not generic? No value to return, we're done
				if (!ReturnType.IsGenericType)
				{
					return SuccessResult.Instance;
				}

				// It has a value? Ok, let's get it
				var value = _TaskResult.Value.Invoke(task);
				return await ConvertValueAsync(value).ConfigureAwait(false);
			}

			private Func<Task, object> TaskResult()
			{
				/*
				 *	(Task Task) =>
				 *	{
				 *		return ((Task<T>)Task).Result;
				 *	}
				 */

				var instance = Expression.Parameter(typeof(Task), "Task");

				var instanceCast = Expression.Convert(instance, ReturnType);
				var property = Expression.Property(instanceCast, nameof(Task<object>.Result));
				var propertyCast = Expression.Convert(property, typeof(object));

				var lambda = Expression.Lambda<Func<Task, object>>(
					propertyCast,
					instance
				);
				return lambda.Compile();
			}
		}
	}
}