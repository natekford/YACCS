using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class MutableMethodInfoCommand : MutableCommand
	{
		public Type GroupType { get; }
		public MethodInfo Method { get; }

		public MutableMethodInfoCommand(ICommandGroup group, MethodInfo method, IEnumerable<string>? extraNames = null)
			: base(method)
		{
			GroupType = group.GetType();
			Method = method;

			foreach (var name in GetFullNames(group, GetDirectCommandNames(method, extraNames)))
			{
				Names.Add(name);
			}
			Attributes.Add(new MethodInfoCommandAttribute(Method));
		}

		public override ICommand ToCommand()
			=> new ImmutableMethodInfoCommand(this);

		private static IEnumerable<string> GetDirectCommandNames(ICustomAttributeProvider method, IEnumerable<string>? extraNames)
		{
			var methodNames = method
				.GetCustomAttributes(true)
				.OfType<ICommandAttribute>()
				.SingleOrDefault()
				?.Names;
			if (methodNames is null)
			{
				return extraNames ?? Enumerable.Empty<string>();
			}
			if (extraNames is null)
			{
				return methodNames ?? Enumerable.Empty<string>();
			}
			return methodNames.Concat(extraNames);
		}

		private static IList<IName> GetFullNames(ICommandGroup group, IEnumerable<string> names)
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

		private sealed class ImmutableMethodInfoCommand : ImmutableCommand
		{
			private readonly ICommandGroup _DO_NOT_USE_THIS_FOR_EXECUTION;
			private readonly Type _GroupType;
			private readonly MethodInfo _Method;

			public ImmutableMethodInfoCommand(MutableMethodInfoCommand mutable)
				: base(mutable, mutable.Method.ReturnType)
			{
				_Method = mutable.Method;
				_GroupType = mutable.GroupType;
				_DO_NOT_USE_THIS_FOR_EXECUTION = CreateGroup();
			}

			public override async Task<ExecutionResult> GetResultAsync(IContext context, object?[] args)
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

			public override bool IsValidContext(IContext context)
				=> _DO_NOT_USE_THIS_FOR_EXECUTION.IsValidContext(context);

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