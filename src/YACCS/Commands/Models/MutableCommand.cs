using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class MutableCommand : MutableEntityBase, IMutableCommand
	{
		public Type GroupType { get; }
		public MethodInfo Method { get; }
		public IList<IName> Names { get; set; }
		public IList<IMutableParameter> Parameters { get; set; }
		IEnumerable<IName> IQueryableCommand.Names => Names;
		private string DebuggerDisplay => $"Name = {Names[0]}, Parameter Count = {Parameters.Count}";

		public MutableCommand(ICommandGroup group, MethodInfo method, IEnumerable<string>? extraNames = null)
			: base(method)
		{
			var names = GetDirectCommandNames(method).Concat(extraNames ?? Enumerable.Empty<string>());

			GroupType = group.GetType();
			Method = method;
			Names = GetFullNames(group, names);
			Parameters = method.GetParameters().Select(x => new MutableParameter(x)).ToList<IMutableParameter>();

			Attributes.Add(new MethodInfoCommandAttribute(Method));
		}

		public MutableCommand(ICommandGroup group, Delegate @delegate, IEnumerable<string>? extraNames = null)
			: this(group, @delegate.Method, extraNames)
		{
			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public static IEnumerable<string> GetDirectCommandNames(MethodInfo method)
		{
			return method
				.GetCustomAttributes()
				.OfType<ICommandAttribute>()
				.SingleOrDefault()
				?.Names ?? Enumerable.Empty<string>();
		}

		public static IList<IName> GetFullNames(ICommandGroup group, IEnumerable<string> names)
		{
			var output = new List<IEnumerable<string>>(names.Select(x => new[] { x }));
			if (output.Count == 0)
			{
				output.Add(Enumerable.Empty<string>());
			}

			var type = group.GetType();
			while (type != null)
			{
				var command = type
					.GetCustomAttributes()
					.OfType<ICommandAttribute>()
					.SingleOrDefault();
				if (command != null)
				{
					var count = output.Count;
					for (var i = 0; i < count; ++i)
					{
						foreach (var name in command.Names)
						{
							output.Add(output[i].Prepend(name));
						}
					}
					output.RemoveRange(0, count);
				}
				type = type.DeclaringType;
			}

			return output.Select(x => new Name(x)).ToList<IName>();
		}

		public ICommand ToCommand()
					=> new ImmutableCommand(this);

		private sealed class ImmutableCommand : ICommand
		{
			private static readonly PropertyInfo _TaskResultProperty =
				typeof(Task<>)
				.GetProperty(nameof(Task<object>.Result));

			private readonly ICommandGroup _DO_NOT_USE_THIS_FOR_EXECUTION;
			private readonly Type _GroupType;
			private readonly MethodInfo _Method;

			public IReadOnlyList<object> Attributes { get; }
			public string Id { get; }
			public IReadOnlyList<IName> Names { get; }
			public IReadOnlyList<IParameter> Parameters { get; }
			public IReadOnlyList<IPrecondition> Preconditions { get; }
			public int Priority { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			IEnumerable<IName> IQueryableCommand.Names => Names;

			public ImmutableCommand(MutableCommand mutable)
			{
				_Method = mutable.Method;
				_GroupType = mutable.GroupType;
				_DO_NOT_USE_THIS_FOR_EXECUTION = CreateGroup();

				Attributes = mutable.Attributes.ToImmutableArray();
				Id = mutable.Id;
				Names = mutable.Names.ToImmutableArray();
				Parameters = mutable.Parameters.Select(x => x.ToParameter()).ToImmutableArray();
				Preconditions = mutable.Get<IPrecondition>().ToImmutableArray();
				Priority = mutable.Get<IPriorityAttribute>().SingleOrDefault()?.Priority ?? 0;
			}

			public async Task<ExecutionResult> GetResultAsync(IContext context, object?[] args)
			{
				try
				{
					var group = CreateGroup();

					await group.BeforeExecutionAsync(this, context).ConfigureAwait(false);
					var value = _Method.Invoke(group, args);
					var result = await ConvertValueAsync(context, value).ConfigureAwait(false);
					await group.AfterExecutionAsync(this, context).ConfigureAwait(false);

					return result;
				}
				catch (Exception e)
				{
					return new ExecutionResult(this, context, new ExceptionResult(e));
				}
			}

			public bool IsValidContext(IContext context)
				=> _DO_NOT_USE_THIS_FOR_EXECUTION.IsValidContext(context);

			private async Task<ExecutionResult> ConvertValueAsync(IContext context, object? value)
			{
				static ExecutionResult ConvertValue(
					ICommand command,
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
				var type = _Method.ReturnType;
				if (type == typeof(void))
				{
					return new ExecutionResult(this, context, SuccessResult.Instance);
				}

				// We're given a task
				if (value is Task task)
				{
					// Let's await it to actually complete it
					await task.ConfigureAwait(false);

					// Not generic? No value to return, we're done
					if (!type.IsGenericType)
					{
						return new ExecutionResult(this, context, SuccessResult.Instance);
					}

					// It has a value? Ok, let's get it
					var result = _TaskResultProperty.GetValue(value);
					return ConvertValue(this, context, result);
				}

				return ConvertValue(this, context, value);
			}

			private ICommandGroup CreateGroup()
			{
				var instance = Activator.CreateInstance(_GroupType);
				if (!(instance is ICommandGroup group))
				{
					throw new InvalidOperationException("Invalid group.");
				}
				return group;
			}
		}
	}
}