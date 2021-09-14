
using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <inheritdoc />
	public interface IPrecondition<in TContext> : IPrecondition where TContext : IContext
	{
		/// <inheritdoc cref="IPrecondition.AfterExecutionAsync(IImmutableCommand, IContext, Exception?)"/>
		Task AfterExecutionAsync(IImmutableCommand command, TContext context, Exception? exception);

		/// <inheritdoc cref="IPrecondition.BeforeExecutionAsync(IImmutableCommand, IContext)"/>
		Task BeforeExecutionAsync(IImmutableCommand command, TContext context);

		/// <inheritdoc cref="IPrecondition.CheckAsync(IImmutableCommand, IContext)"/>
		ValueTask<IResult> CheckAsync(IImmutableCommand command, TContext context);
	}
}