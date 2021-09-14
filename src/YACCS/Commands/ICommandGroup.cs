
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	/// <summary>
	/// A group of commands.
	/// </summary>
	public interface ICommandGroup
	{
		/// <summary>
		/// Handles a command after execution.
		/// </summary>
		/// <param name="command">
		/// <inheritdoc cref="CommandGroup{TContext}.Command" path="/summary"/>
		/// </param>
		/// <param name="context">
		/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
		/// </param>
		/// <param name="result">The result of the command.</param>
		/// <returns></returns>
		Task AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result);

		/// <summary>
		/// Sets up a command before execution.
		/// </summary>
		/// <param name="command">
		/// <inheritdoc cref="CommandGroup{TContext}.Command" path="/summary"/>
		/// </param>
		/// <param name="context">
		/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
		/// </param>
		/// <returns></returns>
		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);
	}
}