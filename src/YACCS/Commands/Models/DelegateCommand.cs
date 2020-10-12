using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.NamedArguments;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class DelegateCommand : Command
	{
		// Delegate commands don't use contexts so this can/should be null by default
		public override Type? ContextType { get; }
		public Delegate Delegate { get; }

		public DelegateCommand(Delegate @delegate, IEnumerable<IName> names)
			: base(@delegate.Method)
		{
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}

			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public DelegateCommand(Delegate @delegate, Type? contextType, IEnumerable<IName> names)
			: base(@delegate.Method)
		{
			ContextType = contextType;
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}

			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public override IEnumerable<IImmutableCommand> ToImmutable()
		{
			var immutable = new ImmutableDelegateCommand(this);
			return this.Get<GenerateNamedArgumentsAttribute>().Any()
				? new[] { immutable, immutable.GenerateNamedArgumentVersion() }
				: new[] { immutable };
		}

		private sealed class ImmutableDelegateCommand : ImmutableCommand
		{
			private readonly Delegate _Delegate;
			private readonly Lazy<Func<object?[], object>> _InvokeDelegate;

			public ImmutableDelegateCommand(DelegateCommand mutable)
				: base(mutable, mutable.Delegate.Method.ReturnType)
			{
				_Delegate = mutable.Delegate;
				_InvokeDelegate = ReflectionUtils.CreateDelegate(CreateInvokeDelegate,
					"invoke delegate");
			}

			public override Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args)
			{
				var value = _InvokeDelegate.Value(args);
				return ConvertValueAsync(context, value);
			}

			private Func<object?[], object> CreateInvokeDelegate()
			{
				/*
				 *	(object?[] Args) =>
				 *	{
				 *		return ((DelegateType)Delegate).Invoke((ParamType)Args[0], (ParamType)Args[1], ...);
				 *	}
				 */

				var method = _Delegate.Method;
				var target = _Delegate.Target;

				var argsExpr = Expression.Parameter(typeof(object?[]), "Args");

				var targetExpr = target is null ? null : Expression.Constant(target);
				var argsCastExpr = method.GetParameters().Select((x, i) =>
				{
					var indexExpr = Expression.Constant(i);
					var accessExpr = Expression.ArrayAccess(argsExpr, indexExpr);
					return Expression.Convert(accessExpr, x.ParameterType);
				});
				var invokeExpr = Expression.Call(targetExpr, method, argsCastExpr);

				Expression bodyExpr = invokeExpr;
				// With a return type of void to keep the Func<object?[], object> declaration
				// we just need to return a null value at the end
				if (ReturnType == typeof(void))
				{
					var nullExpr = Expression.Constant(null);
					var returnLabel = Expression.Label(typeof(object));
					var returnExpr = Expression.Return(returnLabel, nullExpr, typeof(object));
					var returnLabelExpr = Expression.Label(returnLabel, nullExpr);
					bodyExpr = Expression.Block(invokeExpr, returnExpr, returnLabelExpr);
				}

				var lambda = Expression.Lambda<Func<object?[], object>>(bodyExpr, argsExpr);
				return lambda.Compile();
			}
		}
	}
}