using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	/// <summary>
	/// Contains information signifying the score of parsing a command.
	/// </summary>
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class CommandScore : IExecuteResult
	{
		/// <summary>
		/// Indicates that no suitable command was found.
		/// </summary>
		public static CommandScore CommandNotFound { get; }
			= new(null!, null, 0, 0, CommandNotFoundResult.Instance);
		/// <summary>
		/// Indicates that there are too many suitable commands.
		/// </summary>
		public static CommandScore MultiMatch { get; }
			= new(null!, null, 0, 0, MultiMatchHandlingErrorResult.Instance);
		/// <summary>
		/// Indicates that there is an error parsing quotes.
		/// </summary>
		public static CommandScore QuoteMismatch { get; }
			= new(null!, null, 0, 0, QuoteMismatchResult.Instance);

		/// <summary>
		/// The arguments for the command.
		/// </summary>
		/// <remarks>
		/// These will be <see langword="null"/> if the command cannot execute
		/// </remarks>
		public IReadOnlyList<object?>? Args { get; }
		/// <inheritdoc />
		public IImmutableCommand? Command { get; }
		/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
		public IContext Context { get; }
		/// <summary>
		/// Indicates how many segments of the split string have been parsed.
		/// </summary>
		public int Index { get; }
		/// <inheritdoc />
		public IResult InnerResult { get; }
		/// <inheritdoc />
		public IImmutableParameter? Parameter { get; }
		/// <summary>
		/// Indicates the current stage of command execution.
		/// </summary>
		public CommandStage Stage { get; }
		private string DebuggerDisplay
			=> $"Stage = {Stage}, Score = {Index}, Success = {InnerResult.IsSuccess}";

		/// <summary>
		/// Creates a new <see cref="CommandScore"/>.
		/// </summary>
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
		public CommandScore(
			IContext context,
			IImmutableCommand? command,
			CommandStage stage,
			int index,
			IResult result,
			IImmutableParameter? parameter = null,
			IReadOnlyList<object?>? args = null)
		{
			Args = args;
			Command = command;
			Parameter = parameter;
			Context = context;
			InnerResult = result;
			Index = Math.Max(index, 0);
			Stage = stage;
		}
	}
}

namespace YACCS.Commands.CommandScores
{
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
			var result = SuccessResult.Instance;
			const CommandStage STAGE = CommandStage.CanExecute;
			// Subtract start index from int.MaxValue because the more args the less
			// command name parts used, so the less specific the command is
			// i.e. two commands:
			// Echo Colored "text to echo" <-- Score = 2
			// Echo "colored text to echo" <-- Score = 1
			return new(context, command, STAGE, index, result, args: args);
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
			return new(context, command, STAGE, index, result, parameter: parameter);
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
			return new(context, command, STAGE, index, result, parameter: parameter);
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
			var result = InvalidContextResult.Instance;
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
			var result = NotEnoughArgsResult.Instance;
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new(context, command, STAGE, index, result, parameter: parameter);
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
			var result = NotEnoughArgsResult.Instance;
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
			var result = TooManyArgsResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new(context, command, STAGE, index, result);
		}
	}
}