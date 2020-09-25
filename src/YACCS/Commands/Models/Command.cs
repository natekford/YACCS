using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class Command : EntityBase, ICommand
	{
		public abstract Type? ContextType { get; }
		public IList<IName> Names { get; set; }
		public IList<IParameter> Parameters { get; set; }
		IEnumerable<IName> IQueryableCommand.Names => Names;
		private string DebuggerDisplay => $"Name = {Names[0]}, Parameter Count = {Parameters.Count}";

		protected Command(MethodInfo method) : base(method)
		{
			Names = new List<IName>();
			Parameters = method.GetParameters().Select(x => new Parameter(x)).ToList<IParameter>();
		}

		public abstract IImmutableCommand ToCommand();

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		protected abstract class ImmutableCommand : IImmutableCommand
		{
			private readonly bool _IsGeneric;
			private readonly bool _IsVoid;
			private readonly Type _ReturnType;
			private readonly Lazy<Func<Task, object>> _TaskResultDelegate;

			public IReadOnlyList<object> Attributes { get; }
			public Type? ContextType { get; }
			public IReadOnlyList<IName> Names { get; }
			public IReadOnlyList<IImmutableParameter> Parameters { get; }
			public IReadOnlyList<IPrecondition> Preconditions { get; }
			public string PrimaryId { get; }
			public int Priority { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			IEnumerable<IName> IQueryableCommand.Names => Names;
			private string DebuggerDisplay => $"Name = {Names?.FirstOrDefault()?.ToString() ?? PrimaryId}, Parameter Count = {Parameters.Count}";

			protected ImmutableCommand(Command mutable, Type returnType)
			{
				_IsVoid = returnType == typeof(void);
				_IsGeneric = returnType.IsGenericType;
				_ReturnType = returnType;
				_TaskResultDelegate = CreateDelegate(CreateTaskResultDelegate, "task result delegate");

				Attributes = mutable.Attributes.ToImmutableArray();
				ContextType = mutable.ContextType;
				Names = mutable.Names.ToImmutableArray();
				Parameters = mutable.Parameters.Select(x => x.ToParameter()).ToImmutableArray();
				Preconditions = mutable.Get<IPrecondition>().ToImmutableArray();
				PrimaryId = mutable.Get<IIdAttribute>().FirstOrDefault()?.Id ?? Guid.NewGuid().ToString();
				Priority = mutable.Get<IPriorityAttribute>().SingleOrDefault()?.Priority ?? 0;
			}

			public abstract Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args);

			protected static Lazy<T> CreateDelegate<T>(Func<T> createDelegateDelegate, string name)
			{
				return new Lazy<T>(() =>
				{
					try
					{
						return createDelegateDelegate();
					}
					catch (Exception e)
					{
						throw new ArgumentException($"Unable to create {name}.", e);
					}
				});
			}

			protected async Task<ExecutionResult> ConvertValueAsync(IContext context, object? value)
			{
				static ExecutionResult ConvertValue(
					IImmutableCommand command,
					IContext context,
					object? value)
				{
					// We're given a non task result, we can just return that
					if (value is IResult result)
					{
						return new ExecutionResult(command, context, result);
					}
					// What do I do with random values?
					else
					{
						return new ExecutionResult(command, context, new ValueResult(value));
					}
				}

				// Void method. No value to return, we're done
				if (_IsVoid)
				{
					return new ExecutionResult(this, context, SuccessResult.Instance);
				}

				// We're given a task
				if (value is Task task)
				{
					// Let's await it to actually complete it
					await task.ConfigureAwait(false);

					// Not generic? No value to return, we're done
					if (!_IsGeneric)
					{
						return new ExecutionResult(this, context, SuccessResult.Instance);
					}

					// It has a value? Ok, let's get it
					var result = _TaskResultDelegate.Value.Invoke(task);
					return ConvertValue(this, context, result);
				}

				return ConvertValue(this, context, value);
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
				var instanceCastExpr = Expression.Convert(instanceExpr, _ReturnType);
				var propertyExpr = Expression.Property(instanceCastExpr, nameof(Task<object>.Result));
				var propertyCastExpr = Expression.Convert(propertyExpr, typeof(object));

				var lambda = Expression.Lambda<Func<Task, object>>(propertyCastExpr, instanceExpr);
				return lambda.Compile();
			}
		}
	}
}