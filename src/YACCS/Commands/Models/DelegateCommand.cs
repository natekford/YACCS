using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public class DelegateCommand : Command
	{
		public Delegate Delegate { get; }

		public DelegateCommand(
			Delegate @delegate,
			IEnumerable<IReadOnlyList<string>> names,
			Type? contextType = null)
			: this(@delegate, null, contextType ?? typeof(IContext), names)
		{
		}

		public DelegateCommand(Delegate @delegate, IImmutableCommand source)
			: this(@delegate, source, source.ContextType, source.Names)
		{
			Attributes.Add(new GeneratedCommandAttribute(source));
		}

		protected DelegateCommand(
			Delegate @delegate,
			IImmutableCommand? source,
			Type? contextType,
			IEnumerable<IReadOnlyList<string>> names)
			: base(@delegate.Method, source, contextType)
		{
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}

			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		protected override IImmutableCommand MakeImmutable()
			=> new ImmutableDelegateCommand(this);

		protected class ImmutableDelegateCommand : ImmutableCommand
		{
			private readonly Delegate _Delegate;
			private readonly Func<object?[], object> _Execute;

			public ImmutableDelegateCommand(DelegateCommand mutable)
				: base(mutable, mutable.Delegate.Method.ReturnType)
			{
				_Delegate = mutable.Delegate;
				_Execute = ReflectionUtils.CreateDelegate(Execute, "execute");
			}

			public override Task<IResult> ExecuteAsync(IContext context, object?[] args)
			{
				var value = _Execute.Invoke(args);
				return ConvertValueAsync(value);
			}

			protected virtual Func<object?[], object> Execute()
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