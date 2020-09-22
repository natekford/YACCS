using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Attributes;
using YACCS.Results;

namespace YACCS.Commands.Models
{
	public sealed class MutableDelegateCommand : MutableCommand
	{
		public Delegate Delegate { get; }

		public MutableDelegateCommand(Delegate @delegate, IEnumerable<IName> names)
			: base(@delegate.Method)
		{
			Delegate = @delegate;

			foreach (var name in names)
			{
				Names.Add(name);
			}
			Attributes.Add(new DelegateCommandAttribute(@delegate));
		}

		public override ICommand ToCommand()
			=> new ImmutableDelegateCommand(this);

		private sealed class ImmutableDelegateCommand : ImmutableCommand
		{
			private readonly Delegate _Delegate;

			public ImmutableDelegateCommand(MutableDelegateCommand mutable)
				: base(mutable, mutable.Delegate.Method.ReturnType)
			{
				_Delegate = mutable.Delegate;
			}

			public override async Task<ExecutionResult> GetResultAsync(IContext context, object?[] args)
			{
				try
				{
					var value = _Delegate.DynamicInvoke(args);
					return await ConvertValueAsync(context, value).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					return new ExecutionResult(this, context, new ExceptionResult(e));
				}
			}

			public override bool IsValidContext(IContext context)
				=> true;
		}
	}
}