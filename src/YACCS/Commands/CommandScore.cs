using System;
using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class CommandScore : IComparable<CommandScore>, IComparable, INestedResult
	{
		public object?[]? Args { get; }
		public IImmutableCommand? Command { get; }
		public IContext Context { get; }
		public int Priority { get; }
		public IResult Result { get; }
		public int Score { get; }
		public CommandStage Stage { get; }
		private string DebuggerDisplay => $"Stage = {Stage}, Score = {Score}, Success = {Result.IsSuccess}";

		protected CommandScore(
			IImmutableCommand? command,
			IContext context,
			IResult result,
			CommandStage stage,
			int score,
			object?[]? args)
		{
			Args = args;
			Command = command;
			Context = context;
			Priority = command?.Priority ?? 0;
			Result = result;
			Score = score;
			Stage = stage;
		}

		public static CommandScore FromCanExecute(
			IImmutableCommand command,
			IContext context,
			object?[] args)
		{
			var result = SuccessResult.Instance;
			const CommandStage STAGE = CommandStage.CanExecute;
			return new CommandScore(command, context, result, STAGE, int.MaxValue, args);
		}

		public static CommandScore FromCorrectArgCount(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = SuccessResult.Instance;
			const CommandStage STAGE = CommandStage.CorrectArgCount;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedOptionalArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance;
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedParameterPrecondition(
			IImmutableCommand command,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedParameterPrecondition;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedPrecondition(
			IImmutableCommand command,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedPrecondition;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedTypeReader(
			IImmutableCommand command,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromInvalidContext(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = InvalidContextResult.Instance;
			const CommandStage STAGE = CommandStage.BadContext;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromNotEnoughArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromNotFound()
		{
			var result = CommandNotFoundResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(null, null!, result, STAGE, int.MinValue, null);
		}

		public static CommandScore FromQuoteMismatch()
		{
			var result = QuoteMismatchResult.Instance;
			const CommandStage STAGE = CommandStage.QuoteMismatch;
			return new CommandScore(null, null!, result, STAGE, int.MinValue, null);
		}

		public static CommandScore FromTooManyArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = TooManyArgsResult.Instance;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public int CompareTo(object obj)
		{
			if (obj is null)
			{
				return 1;
			}
			if (obj is CommandScore other)
			{
				return CompareTo(other);
			}
			throw new ArgumentException($"Object is not a {nameof(CommandScore)}.");
		}

		public int CompareTo(CommandScore other)
		{
			var stage = Stage.CompareTo(other.Stage);
			if (stage != 0)
			{
				return stage;
			}

			var success = Result.IsSuccess.CompareTo(other.Result.IsSuccess);
			if (success != 0)
			{
				return success;
			}

			var priority = Priority.CompareTo(other.Priority);
			if (priority != 0)
			{
				return priority;
			}

			return Score.CompareTo(other.Score);
		}
	}
}