
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	/// <inheritdoc cref="ICommandGroup" />
	public interface ICommandGroup<in TContext> : ICommandGroup where TContext : IContext
	{
		/// <inheritdoc cref="ICommandGroup.AfterExecutionAsync(IImmutableCommand, IContext, IResult)"/>
		Task AfterExecutionAsync(IImmutableCommand command, TContext context, IResult result);

		/// <inheritdoc cref="ICommandGroup.BeforeExecutionAsync(IImmutableCommand, IContext)"/>
		Task BeforeExecutionAsync(IImmutableCommand command, TContext context);
	}
}