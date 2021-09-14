
using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a precondition.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public abstract class Precondition<TContext>
		: GroupablePrecondition, IPrecondition<TContext> where TContext : IContext
	{
		/// <inheritdoc />
		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context, Exception? exception)
			=> Task.CompletedTask;

		/// <inheritdoc />
		public virtual Task BeforeExecutionAsync(IImmutableCommand command, TContext context)
			=> Task.CompletedTask;

		/// <inheritdoc />
		public abstract ValueTask<IResult> CheckAsync(IImmutableCommand command, TContext context);

		Task IPrecondition.AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception)
			=> this.HandleAsync<TContext>(command, context, (c, ctx) => AfterExecutionAsync(c, ctx, exception));

		Task IPrecondition.BeforeExecutionAsync(IImmutableCommand command, IContext context)
			=> this.HandleAsync<TContext>(command, context, BeforeExecutionAsync);

		ValueTask<IResult> IPrecondition.CheckAsync(IImmutableCommand command, IContext context)
			=> this.CheckAsync<TContext>(command, context, CheckAsync);
	}
}