using YACCS.Commands.Models;
using YACCS.Results;

// Different namespace since they're extension methods
namespace YACCS.Commands.CommandScores;

/// <summary>
/// Utilities for creating <see cref="CommandScore"/>.
/// </summary>
public static class CommandScoreUtils
{
	/// <summary>
	/// Creates a new <see cref="CommandScore"/> with the stage set to
	/// <see cref="CommandStage.CanExecute"/>.
	/// </summary>
	/// <inheritdoc cref="CommandScore(IContext, IImmutableCommand?, CommandStage, int, IResult, IImmutableParameter?,IReadOnlyList{object?}?)"/>
	public static CommandScore CanExecute(
		this IContext context,
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
		this IContext context,
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
		this IContext context,
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
		this IContext context,
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
		this IContext context,
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
		this IContext context,
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
		this IContext context,
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
		this IContext context,
		IImmutableCommand command,
		int index)
	{
		var result = CachedResults.TooManyArgs;
		const CommandStage STAGE = CommandStage.BadArgCount;
		return new(context, command, STAGE, index, result);
	}
}