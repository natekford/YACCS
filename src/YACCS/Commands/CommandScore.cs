#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class CommandScore : IExecuteResult
	{
		// This class is a mess
		public static CommandScore CommandNotFound { get; }
			= new(null, null, null!, CommandNotFoundResult.Instance, 0, 0, null);
		public static CommandScore MultiMatch { get; }
			= new(null, null, null!, MultiMatchHandlingErrorResult.Instance, 0, 0, null);
		public static CommandScore QuoteMismatch { get; }
			= new(null, null, null!, QuoteMismatchResult.Instance, 0, 0, null);

		public object?[]? Args { get; }
		public IImmutableCommand? Command { get; }
		public IContext Context { get; }
		public IResult InnerResult { get; }
		public IImmutableParameter? Parameter { get; }
		public int Priority { get; }
		public int Score { get; }
		public CommandStage Stage { get; }
		private string DebuggerDisplay => $"Stage = {Stage}, Score = {Score}, Success = {InnerResult.IsSuccess}";

		/// <summary>
		/// Creates a new <see cref="CommandScore"/>.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		/// <param name="context">
		/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
		/// </param>
		/// <param name="result"></param>
		/// <param name="stage"></param>
		/// <param name="score"></param>
		/// <param name="args"></param>
		protected CommandScore(
			IImmutableCommand? command,
			IImmutableParameter? parameter,
			IContext context,
			IResult result,
			CommandStage stage,
			int score,
			object?[]? args)
		{
			Args = args;
			Command = command;
			Parameter = parameter;
			Context = context;
			Priority = command?.Priority ?? 0;
			InnerResult = result;
			Score = Math.Max(score, 0);
			Stage = stage;
		}

		public static CommandScore FromCanExecute(
			IImmutableCommand command,
			IContext context,
			object?[] args,
			int score)
		{
			var result = SuccessResult.Instance;
			const CommandStage STAGE = CommandStage.CanExecute;
			// Subtract start index from int.MaxValue because the more args the less
			// command name parts used, so the less specific the command is
			// i.e. two commands:
			// Echo Colored "text to echo" <-- Score = 2
			// Echo "colored text to echo" <-- Score = 1
			return new(command, null, context, result, STAGE, score, args);
		}

		public static CommandScore FromFailedOptionalArgs(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance;
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedParameterPrecondition(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedParameterPrecondition;
			return new(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedPrecondition(
			IImmutableCommand command,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedPrecondition;
			return new(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedTypeReader(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromInvalidContext(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = InvalidContextResult.Instance;
			const CommandStage STAGE = CommandStage.BadContext;
			return new(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromNotEnoughArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromTooManyArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = TooManyArgsResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new(command, null, context, result, STAGE, score, null);
		}
	}
}