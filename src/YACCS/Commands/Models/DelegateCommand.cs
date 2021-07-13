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
			: this(@delegate, contextType ?? typeof(IContext), null, names)
		{
		}

		public DelegateCommand(Delegate @delegate, IImmutableCommand source)
			: this(@delegate, source.ContextType, source, source.Names)
		{
			Attributes.Add(new GeneratedCommandAttribute(source));
		}

		protected DelegateCommand(
			Delegate @delegate,
			Type contextType,
			IImmutableCommand? source,
			IEnumerable<IReadOnlyList<string>> names)
			: base(@delegate.Method, contextType, source)
		{
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}

			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public override IImmutableCommand ToImmutable()
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

			public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
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