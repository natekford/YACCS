using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class Precondition<TContext>
		: GroupablePrecondition, IPrecondition<TContext> where TContext : IContext
	{
		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context, Exception? exception)
			=> Task.CompletedTask;

		public virtual Task BeforeExecutionAsync(IImmutableCommand command, TContext context)
			=> Task.CompletedTask;

		public abstract Task<IResult> CheckAsync(IImmutableCommand command, TContext context);

		Task IPrecondition.AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> this.HandleAsync<TContext>(command, context, (c, ctx) => AfterExecutionAsync(c, ctx, exception));

		Task IPrecondition.BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> this.HandleAsync<TContext>(command, context, BeforeExecutionAsync);

		Task<IResult> IPrecondition.CheckAsync(IImmutableCommand command, IContext context)
			=> this.HandleAsync<TContext>(command, context, CheckAsync);
	}
}