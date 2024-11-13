using System.Collections.Generic;
using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Contains information signifying the score of parsing a command.
/// </summary>
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

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.CanExecute"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore CanExecute(
		IContext context,
		IImmutableCommand command,
		int index,
		IReadOnlyList<object?>? args)
	{
		var result = CachedResults.Success;
		const CommandStage STAGE = CommandStage.CanExecute;
		// Subtract start index from int.MaxValue because the more args the less
		// command name parts used, so the less specific the command is
		// i.e. two commands:
		// Echo Colored "text to echo" <-- Score = 2
		// Echo "colored text to echo" <-- Score = 1
		return new(context, command, STAGE, index, result, Args: args);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.FailedParameterPrecondition"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore FailedParameterPrecondition(
		IContext context,
		IImmutableCommand command,
		int index,
		IResult result,
		IImmutableParameter parameter)
	{
		const CommandStage STAGE = CommandStage.FailedParameterPrecondition;
		return new(context, command, STAGE, index, result, Parameter: parameter);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.FailedPrecondition"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore FailedPrecondition(
		IContext context,
		IImmutableCommand command,
		int index,
		IResult result)
	{
		const CommandStage STAGE = CommandStage.FailedPrecondition;
		return new(context, command, STAGE, index, result);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.FailedTypeReader"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore FailedTypeReader(
		IContext context,
		IImmutableCommand command,
		int index,
		IResult result,
		IImmutableParameter parameter)
	{
		const CommandStage STAGE = CommandStage.FailedTypeReader;
		return new(context, command, STAGE, index, result, Parameter: parameter);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.BadContext"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore InvalidContext(
		IContext context,
		IImmutableCommand command,
		int index)
	{
		var result = CachedResults.InvalidContext;
		const CommandStage STAGE = CommandStage.BadContext;
		return new(context, command, STAGE, index, result);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.FailedTypeReader"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore MissingParameterValue(
		IContext context,
		IImmutableCommand command,
		int index,
		IImmutableParameter parameter)
	{
		var result = CachedResults.NotEnoughArgs;
		const CommandStage STAGE = CommandStage.FailedTypeReader;
		return new(context, command, STAGE, index, result, Parameter: parameter);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.BadArgCount"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore NotEnoughArgs(
		IContext context,
		IImmutableCommand command,
		int index)
	{
		var result = CachedResults.NotEnoughArgs;
		const CommandStage STAGE = CommandStage.BadArgCount;
		return new(context, command, STAGE, index, result);
	}

	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.BadArgCount"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore TooManyArgs(
		IContext context,
		IImmutableCommand command,
		int index)
	{
		var result = CachedResults.TooManyArgs;
		const CommandStage STAGE = CommandStage.BadArgCount;
		return new(context, command, STAGE, index, result);
	}
}