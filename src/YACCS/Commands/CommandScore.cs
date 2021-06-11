using System;
using System.Diagnostics;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class CommandScore : IComparable<CommandScore>, IComparable, ICommandResult
	{
		// This class is a mess
		public static CommandScore CommandNotFound { get; }
			= new CommandScore(null, null, null!, CommandNotFoundResult.Instance.Sync, 0, 0, null);
		public static Task<ICommandResult> CommandNotFoundTask { get; }
			= Task.FromResult<ICommandResult>(CommandNotFound);
		public static CommandScore MultiMatch { get; }
			= new CommandScore(null, null, null!, MultiMatchHandlingErrorResult.Instance.Sync, 0, 0, null);
		public static Task<ICommandResult> MultiMatchTask { get; }
			= Task.FromResult<ICommandResult>(MultiMatch);
		public static CommandScore QuoteMismatch { get; }
			= new CommandScore(null, null, null!, QuoteMismatchResult.Instance.Sync, 0, 0, null);
		public static Task<ICommandResult> QuoteMismatchTask { get; }
			= Task.FromResult<ICommandResult>(QuoteMismatch);

		public object?[]? Args { get; }
		public IImmutableCommand? Command { get; }
		public IContext Context { get; }
		public IResult InnerResult { get; }
		public IImmutableParameter? Parameter { get; }
		public int Priority { get; }
		public int Score { get; }
		public CommandStage Stage { get; }
		public bool IsSuccess => InnerResult.IsSuccess;
		public string Response => InnerResult.Response;

		private string DebuggerDisplay => $"Stage = {Stage}, Score = {Score}, Success = {InnerResult.IsSuccess}";

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

			static double GetModifier(CommandStage stage)
			{
				return stage switch
				{
					CommandStage.BadContext => 0,
					CommandStage.BadArgCount => 0.1,
					CommandStage.FailedPrecondition => 0.6,
					CommandStage.FailedTypeReader => 0.7,
					CommandStage.FailedParameterPrecondition => 0.8,
					CommandStage.CanExecute => 1,
					_ => throw new ArgumentOutOfRangeException(nameof(stage)),
				};
			}

			var modifierA = GetModifier(a.Stage);
			var modifierB = GetModifier(b.Stage);

			var scoreA = modifierA * (a.Score + Math.Pow(a.Priority, modifierA));
			var scoreB = modifierB * (b.Score + Math.Pow(b.Priority, modifierB));
			return scoreA.CompareTo(scoreB);
		}

		public static CommandScore FromCanExecute(
			IImmutableCommand command,
			IContext context,
			object?[] args)
		{
			var result = SuccessResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.CanExecute;
			return new CommandScore(command, null, context, result, STAGE, int.MaxValue, args);
		}

		public static CommandScore FromFailedOptionalArgs(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new CommandScore(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedParameterPrecondition(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedParameterPrecondition;
			return new CommandScore(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedPrecondition(
			IImmutableCommand command,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedPrecondition;
			return new CommandScore(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromFailedTypeReader(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			IResult result,
			int score)
		{
			const CommandStage STAGE = CommandStage.FailedTypeReader;
			return new CommandScore(command, parameter, context, result, STAGE, score, null);
		}

		public static CommandScore FromInvalidContext(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = InvalidContextResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadContext;
			return new CommandScore(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromNotEnoughArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = NotEnoughArgsResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, null, context, result, STAGE, score, null);
		}

		public static CommandScore FromTooManyArgs(
			IImmutableCommand command,
			IContext context,
			int score)
		{
			var result = TooManyArgsResult.Instance.Sync;
			const CommandStage STAGE = CommandStage.BadArgCount;
			return new CommandScore(command, null, context, result, STAGE, score, null);
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