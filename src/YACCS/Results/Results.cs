using System.Threading.Tasks;

namespace YACCS.Results
{
	public class CanceledResult : Result
	{
		public static IResult Instance { get; } = new CanceledResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected CanceledResult() : base(false, "An operation was canceled.")
		{
		}
	}

	public class CommandNotFoundResult : Result
	{
		public static IResult Instance { get; } = new CommandNotFoundResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected CommandNotFoundResult() : base(false, "Unable to find a matching command.")
		{
		}
	}

	public class ExceptionAfterCommandResult : Result
	{
		public static IResult Instance { get; } = new ExceptionAfterCommandResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected ExceptionAfterCommandResult() : base(false, "An exception occurred after a command was executed.")
		{
		}
	}

	public class ExceptionDuringCommandResult : Result
	{
		public static IResult Instance { get; } = new ExceptionDuringCommandResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected ExceptionDuringCommandResult() : base(false, "An exception occurred while a command was executing.")
		{
		}
	}

	public class InvalidContextResult : Result
	{
		public static IResult Instance { get; } = new InvalidContextResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected InvalidContextResult() : base(false, "Invalid context type.")
		{
		}
	}

	public class InvalidParameterResult : Result
	{
		public static IResult Instance { get; } = new InvalidParameterResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected InvalidParameterResult() : base(false, "Invalid parameter type.")
		{
		}
	}

	public class MultiMatchHandlingErrorResult : Result
	{
		public static IResult Instance { get; } = new MultiMatchHandlingErrorResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected MultiMatchHandlingErrorResult() : base(false, "Multiple commands match.")
		{
		}
	}

	public class NotEnoughArgsResult : Result
	{
		public static IResult Instance { get; } = new NotEnoughArgsResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected NotEnoughArgsResult() : base(false, "Not enough arguments provided.")
		{
		}
	}

	public class NullParameterResult : Result
	{
		public static IResult Instance { get; } = new NullParameterResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected NullParameterResult() : base(false, "Parameter is null.")
		{
		}
	}

	public class QuoteMismatchResult : Result
	{
		public static IResult Instance { get; } = new QuoteMismatchResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected QuoteMismatchResult() : base(false, "Unable to parse arguments: quote mismatch.")
		{
		}
	}

	public class SuccessResult : Result
	{
		public static IResult Instance { get; } = new SuccessResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected SuccessResult() : base(true, "")
		{
		}
	}

	public class TimedOutResult : Result
	{
		public static IResult Instance { get; } = new TimedOutResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected TimedOutResult() : base(false, "An operation timed out.")
		{
		}
	}

	public class TooManyArgsResult : Result
	{
		public static IResult Instance { get; } = new TooManyArgsResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected TooManyArgsResult() : base(false, "Too many arguments provided.")
		{
		}
	}
}