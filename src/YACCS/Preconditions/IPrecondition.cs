using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// Defines methods for validating a precondition and has methods that will be called
	/// before and after a command has been executed.
	/// </summary>
	public interface IPrecondition : IGroupablePrecondition
	{
		/// <summary>
		/// This method is called after <paramref name="command"/> has been executed.
		/// </summary>
		/// <param name="command">The command about to be executed.</param>
		/// <param name="context">The context invoking the command.</param>
		/// <param name="exception">The exception that occurred during command execution.</param>
		/// <returns></returns>
		Task AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception);

		/// <summary>
		/// This method is called before <paramref name="command"/> has been executed.
		/// </summary>
		/// <param name="command">The command about to be executed.</param>
		/// <param name="context">The context invoking the command.</param>
		/// <returns></returns>
		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);

		/// <summary>
		/// Determines if <paramref name="context"/> can invoke <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The command about to be executed.</param>
		/// <param name="context">The context invoking the command.</param>
		/// <returns>A result indicating success or failure.</returns>
		/// <remarks>
		/// This method should NOT handle actions like ratelimits for specific commands,
		/// use either <see cref="BeforeExecutionAsync(IImmutableCommand, IContext)"/>
		/// or <see cref="AfterExecutionAsync(IImmutableCommand, IContext, Exception?)"/>
		/// for that.
		/// </remarks>
		ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}