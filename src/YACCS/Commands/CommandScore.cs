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
/// <param name="context">
/// <inheritdoc cref="Context" path="/summary"/>
/// </param>
/// <param name="command">
/// <inheritdoc cref="Command" path="/summary"/>
/// </param>
/// <param name="stage">
/// <inheritdoc cref="Stage" path="/summary"/>
/// </param>
/// <param name="index">
/// <inheritdoc cref="Index" path="/summary"/>
/// </param>
/// <param name="result">
/// <inheritdoc cref="InnerResult" path="/summary"/>
/// </param>
/// <param name="parameter">
/// <inheritdoc cref="Parameter" path="/summary"/>
/// </param>
/// <param name="args">
/// <inheritdoc cref="Args" path="/summary"/>
/// </param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class CommandScore(
	IContext context,
	IImmutableCommand? command,
	CommandStage stage,
	int index,
	IResult result,
	IImmutableParameter? parameter = null,
	IReadOnlyList<object?>? args = null) : IExecuteResult
{
	/// <summary>
	/// Indicates that no suitable command was found.
	/// </summary>
	public static CommandScore CommandNotFound { get; }
		= new(null!, null, 0, 0, Results.CommandNotFound.Instance);
	/// <summary>
	/// Indicates that there are too many suitable commands.
	/// </summary>
	public static CommandScore MultiMatch { get; }
		= new(null!, null, 0, 0, MultiMatchHandlingError.Instance);
	/// <summary>
	/// Indicates that there is an error parsing quotes.
	/// </summary>
	public static CommandScore QuoteMismatch { get; }
		= new(null!, null, 0, 0, Results.QuoteMismatch.Instance);

	/// <summary>
	/// The arguments for the command.
	/// </summary>
	/// <remarks>
	/// These will be <see langword="null"/> if the command cannot execute
	/// </remarks>
	public IReadOnlyList<object?>? Args { get; } = args;
	/// <inheritdoc />
	public IImmutableCommand? Command { get; } = command;
	/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
	public IContext Context { get; } = context;
	/// <summary>
	/// Indicates how many segments of the split string have been parsed.
	/// </summary>
	public int Index { get; } = Math.Max(index, 0);
	/// <inheritdoc />
	public IResult InnerResult { get; } = result;
	/// <inheritdoc />
	public IImmutableParameter? Parameter { get; } = parameter;
	/// <summary>
	/// Indicates the current stage of command execution.
	/// </summary>
	public CommandStage Stage { get; } = stage;
	private string DebuggerDisplay
		=> $"Stage = {Stage}, Score = {Index}, Success = {InnerResult.IsSuccess}";
}