using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
			private readonly Lazy<Func<ICommandGroup>> _CreateDelegate;
			private readonly ICommandGroup _DO_NOT_USE_THIS_FOR_EXECUTION;
			private readonly Type _GroupType;
			private readonly Lazy<Func<ICommandGroup, object?[], object>> _InvokeDelegate;
			private readonly MethodInfo _Method;

			public ImmutableMethodInfoCommand(MutableMethodInfoCommand mutable)
				: base(mutable, mutable.Method.ReturnType)
			{
				_CreateDelegate = new Lazy<Func<ICommandGroup>>(() =>
				{
					var ctor = _GroupType.GetConstructor(Type.EmptyTypes);
					var ctorExpr = Expression.New(ctor);
					var lambda = Expression.Lambda<Func<ICommandGroup>>(ctorExpr);
					return lambda.Compile();
				});
				_InvokeDelegate = new Lazy<Func<ICommandGroup, object?[], object>>(() =>
				{
					/*
					 *	(ICommandGroup Group, object?[] Args) =>
					 *	{
					 *		return ((GroupType)Group).Method((ParamType)Args[0], (ParamType)Args[1], ...);
					 *	}
					 */

					var instanceExpr = Expression.Parameter(typeof(ICommandGroup), "Group");
					var argsExpr = Expression.Parameter(typeof(object?[]), "Args");

					var instanceCastExpr = Expression.Convert(instanceExpr, _GroupType);
					var argsCastExpr = _Method.GetParameters().Select((x, i) =>
					{
						var indexExpr = Expression.Constant(i);
						var accessExpr = Expression.ArrayAccess(argsExpr, indexExpr);
						return Expression.Convert(accessExpr, x.ParameterType);
					});
					var invokeExpr = Expression.Call(instanceCastExpr, _Method, argsCastExpr);

					var lambda = Expression.Lambda<Func<ICommandGroup, object?[], object>>(invokeExpr, instanceExpr, argsExpr);
					return lambda.Compile();
				});

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

					var value = _InvokeDelegate.Value.Invoke(group, args);
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
				var instance = _CreateDelegate.Value.Invoke();
				if (!(instance is ICommandGroup group))
				{
					throw new InvalidOperationException("Invalid group.");
				}
				return group;
			}
		}
	}
}