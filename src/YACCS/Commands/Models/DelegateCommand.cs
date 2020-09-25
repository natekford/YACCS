using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class DelegateCommand : Command
	{
		// Delegate commands don't use contexts
		public override Type? ContextType => null;
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

		public override IImmutableCommand ToCommand()
			=> new ImmutableDelegateCommand(this);

		private sealed class ImmutableDelegateCommand : ImmutableCommand
		{
			private readonly Delegate _Delegate;
			private readonly Lazy<Func<object?[], object>> _InvokeDelegate;

			public ImmutableDelegateCommand(DelegateCommand mutable)
				: base(mutable, mutable.Delegate.Method.ReturnType)
			{
				_Delegate = mutable.Delegate;
				_InvokeDelegate = CreateDelegate(CreateInvokeDelegate, "invoke delegate");
			}

			public override async Task<ExecutionResult> ExecuteAsync(IContext context, object?[] args)
			{
				try
				{
					var value = _InvokeDelegate.Value(args);
					return await ConvertValueAsync(context, value).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					return new ExecutionResult(this, context, new ExceptionResult(e));
				}
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

				var targetExpr = Expression.Constant(target);
				var argsCastExpr = method.GetParameters().Select((x, i) =>
				{
					var indexExpr = Expression.Constant(i);
					var accessExpr = Expression.ArrayAccess(argsExpr, indexExpr);
					return Expression.Convert(accessExpr, x.ParameterType);
				});
				var invokeExpr = Expression.Call(targetExpr, method, argsCastExpr);

				var lambda = Expression.Lambda<Func<object?[], object>>(invokeExpr, argsExpr);
				return lambda.Compile();
			}
		}
	}
}