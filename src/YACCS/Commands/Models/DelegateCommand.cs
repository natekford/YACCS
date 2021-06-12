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
	public class DelegateCommand : Command
	{
		// Delegate commands don't use contexts so this can/should be null by default
		public override Type? ContextType { get; }
		public Delegate Delegate { get; }

		public DelegateCommand(Delegate @delegate, IEnumerable<IReadOnlyList<string>> names)
			: base(@delegate.Method)
		{
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}

			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public DelegateCommand(Delegate @delegate, Type? contextType, IEnumerable<IReadOnlyList<string>> names)
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

		protected class ImmutableDelegateCommand : ImmutableCommand
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

			public override Task<IResult> ExecuteAsync(IContext context, object?[] args)
			{
				var value = _InvokeDelegate.Value(args);
				return ConvertValueAsync(value);
			}

			protected virtual Func<object?[], object> CreateInvokeDelegate()
			{
				/*
				 *	(object?[] Args) =>
				 *	{
				 *		return ((DelegateType)Delegate).Invoke((ParamType)Args[0], (ParamType)Args[1], ...);
				 *	}
				 */

				var instance = _Delegate.Target is null ? null : Expression.Constant(_Delegate.Target);

				var (body, args) = instance.CreateInvokeExpressionFromObjectArrayArgs(_Delegate.Method);

				var lambda = Expression.Lambda<Func<object?[], object>>(
					body,
					args
				);
				return lambda.Compile();
			}
		}
	}
}