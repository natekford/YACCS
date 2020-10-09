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
		public IResult InnerResult { get; }
		public int Priority { get; }
		public int Score { get; }
		public CommandStage Stage { get; }
		private string DebuggerDisplay => $"Stage = {Stage}, Score = {Score}, Success = {InnerResult.IsSuccess}";

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
			InnerResult = result;
			Score = score;
			Stage = stage;
		}

		public static int CompareTo(CommandScore? a, CommandScore? b)
		{
			if (a is null)
			{
				if (b is null)
				{
					return 0;
				}

				return -1;
			}
			if (b is null)
			{
				return 1;
			}

			var stage = ((int)a.Stage).CompareTo((int)b.Stage);
			if (stage != 0)
			{
				return stage;
			}

			var priority = a.Priority.CompareTo(b.Priority);
			if (priority != 0)
			{
				return priority;
			}

			return a.Score.CompareTo(b.Score);
		}

		public static CommandScore FromCanExecute(
			IImmutableCommand command,
			IContext context,
			object?[] args)
		{
			var result = SuccessResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.CanExecute;
			return new CommandScore(command, context, result, STAGE, int.MaxValue, args);
		}

		public static CommandScore FromFailedOptionalArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance.Sync;
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
			var result = InvalidContextResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadContext;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromNotEnoughArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore FromTooManyArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = TooManyArgsResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, context, result, STAGE, score, null);
		}

		public static CommandScore? Max(CommandScore? a, CommandScore? b)
			=> a > b ? a : b;

		public static bool operator !=(CommandScore? a, CommandScore? b)
			=> !(a == b);

		public static bool operator <(CommandScore? a, CommandScore? b)
			=> CompareTo(a, b) < 0;

		public static bool operator <=(CommandScore? a, CommandScore? b)
			=> !(a > b);

		public static bool operator ==(CommandScore? a, CommandScore? b)
			=> CompareTo(a, b) == 0;

		public static bool operator >(CommandScore? a, CommandScore? b)
			=> CompareTo(a, b) > 0;

		public static bool operator >=(CommandScore? a, CommandScore? b)
			=> !(a < b);

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
			=> CompareTo(this, other);

		public override bool Equals(object obj)
			=> obj is CommandScore other && CompareTo(other) == 0;

		public override int GetHashCode()
			=> HashCode.Combine(Stage, InnerResult.IsSuccess, Priority, Score);
	}
}