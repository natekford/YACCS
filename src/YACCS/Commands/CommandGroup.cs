using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public abstract class CommandGroup<TContext> : ICommandGroup<TContext> where TContext : IContext
	{
		public IImmutableCommand Command { get; private set; } = default!;
		public TContext Context { get; private set; } = default!;

		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context)
			=> Task.CompletedTask;

		public virtual Task BeforeExecutionAsync(IImmutableCommand command, TContext context)
		{
			if (command is null)
			{
				throw new ArgumentNullException(nameof(command));
			}
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (!(context is TContext castedContext))
			{
				var msg = $"Invalid context; expected {typeof(TContext).Name}, received {context.GetType().Name}.";
				throw new ArgumentException(msg, nameof(context));
			}

			Command = command;
			Context = castedContext;
			return Task.CompletedTask;
		}

		public virtual Task OnCommandBuildingAsync(IList<ICommand> commands)
			=> Task.CompletedTask;

		Task ICommandGroup.AfterExecutionAsync(IImmutableCommand command, IContext context)
			=> AfterExecutionAsync(command, (TContext)context);

		Task ICommandGroup.BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> BeforeExecutionAsync(command, (TContext)context);
	}
}