using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class DelegateCommand : Command
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
		}

		private DelegateCommand(
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

			Attributes.Add(@delegate);
		}

		public override IImmutableCommand ToImmutable()
			=> new ImmutableDelegateCommand(this);

		private sealed class ImmutableDelegateCommand : ImmutableCommand
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
				=> ConvertValueAsync(_Execute.Invoke(args));

			private Func<object?[], object> Execute()
			{
				return ExpressionUtils.GetInvokeFromObjectArray<Func<object?[], object>>(
					_Delegate.Target is null ? null : Expression.Constant(_Delegate.Target),
					_Delegate.Method
				);
			}
		}
	}
}