using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public abstract class CommandGroup<TContext> : ICommandGroup<TContext> where TContext : IContext
	{
		public IImmutableCommand Command { get; private set; } = default!;
		public TContext Context { get; private set; } = default!;

		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context, IResult result)
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

			Command = command;
			Context = context;
			return Task.CompletedTask;
		}

		public virtual Task OnCommandBuildingAsync(IList<ICommand> commands)
			=> Task.CompletedTask;

		Task ICommandGroup.AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result)
		{
			if (context is null)
			{
				return AfterExecutionAsync(command, default!, result);
			}
			if (context is not TContext tContext)
			{
				throw InvalidContext(context);
			}
			return AfterExecutionAsync(command, tContext, result);
		}

		Task ICommandGroup.BeforeExecutionAsync(IImmutableCommand command, IContext context)
		{
			if (context is null)
			{
				return BeforeExecutionAsync(command, default!);
			}
			if (context is not TContext tContext)
			{
				throw InvalidContext(context);
			}
			return BeforeExecutionAsync(command, tContext);
		}

		private static ArgumentException InvalidContext(IContext context)
		{
			return new ArgumentException(
				"Invalid context; " +
				$"expected {typeof(TContext).Name}, " +
				$"received {context.GetType().Name}.", nameof(context));
		}
	}
}