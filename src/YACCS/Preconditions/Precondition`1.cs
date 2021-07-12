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
		public virtual ValueTask AfterExecutionAsync(IImmutableCommand command, TContext context, Exception? exception)
			=> new();

		public virtual ValueTask BeforeExecutionAsync(IImmutableCommand command, TContext context)
			=> new();

		public abstract ValueTask<IResult> CheckAsync(IImmutableCommand command, TContext context);

		ValueTask IPrecondition.AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> this.HandleAsync<TContext>(command, context, (c, ctx) => AfterExecutionAsync(c, ctx, exception));

		ValueTask IPrecondition.BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> this.HandleAsync<TContext>(command, context, BeforeExecutionAsync);

		ValueTask<IResult> IPrecondition.CheckAsync(IImmutableCommand command, IContext context)
			=> this.HandleAsync<TContext>(command, context, CheckAsync);
	}
}