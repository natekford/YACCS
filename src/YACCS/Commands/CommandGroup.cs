using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public abstract class CommandGroup<T> : ICommandGroup where T : IContext
	{
		public ICommand Command { get; private set; } = default!;
		public T Context { get; private set; } = default!;

		public virtual Task AfterExecutionAsync(ICommand command, IContext context)
		{
			if (command is null)
			{
				throw new ArgumentNullException(nameof(command));
			}
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (!(context is T castedContext))
			{
				var msg = $"Invalid context; expected {typeof(T).Name}, received {context.GetType().Name}.";
				throw new ArgumentException(msg, nameof(context));
			}

			Command = command;
			Context = castedContext;
			return Task.CompletedTask;
		}

		public virtual Task BeforeExecutionAsync(ICommand command, IContext context)
			=> Task.CompletedTask;

		public virtual bool IsValidContext(IContext context)
			=> context is T;

		public virtual Task OnCommandBuildingAsync(IList<IMutableCommand> commands)
			=> Task.CompletedTask;
	}
}