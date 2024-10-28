using System.Collections.Generic;
using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Contains information signifying the score of parsing a command.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandScore"/>.
/// </remarks>
/// <param name="Context">
/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
/// </param>
/// <param name="Command">
/// <inheritdoc cref="IExecuteResult.Command" path="/summary"/>
/// </param>
/// <param name="Stage">
/// Indicates the current stage of command execution.
/// </param>
/// <param name="Index">
/// Indicates how many segments of the split string have been parsed.
/// </param>
/// <param name="Result">
/// <inheritdoc cref="INestedResult.InnerResult" path="/summary"/>
/// </param>
/// <param name="Parameter">
/// <inheritdoc cref="IExecuteResult.Parameter" path="/summary"/>
/// </param>
/// <param name="Args">
/// The arguments for the command.
/// These will be <see langword="null"/> if the command cannot execute
/// </param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public record CommandScore(
	IContext Context,
	IImmutableCommand? Command,
	CommandStage Stage,
	int Index,
	IResult Result,
	IImmutableParameter? Parameter = null,
	IReadOnlyList<object?>? Args = null
) : IExecuteResult
{
	/// <summary>
	/// Indicates that no suitable command was found.
	/// </summary>
	public static CommandScore CommandNotFound { get; }
		= new(null!, null, 0, 0, CachedResults.CommandNotFound);
	/// <summary>
	/// Indicates that there are too many suitable commands.
	/// </summary>
	public static CommandScore MultiMatch { get; }
		= new(null!, null, 0, 0, CachedResults.MultiMatchHandlingError);
	/// <summary>
	/// Indicates that there is an error parsing quotes.
	/// </summary>
	public static CommandScore QuoteMismatch { get; }
		= new(null!, null, 0, 0, CachedResults.QuoteMismatch);

	private string DebuggerDisplay
		=> $"Stage = {Stage}, Score = {Index}, Success = {Result.IsSuccess}";

	IResult INestedResult.InnerResult => Result;
}