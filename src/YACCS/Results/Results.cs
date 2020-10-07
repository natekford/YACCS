namespace YACCS.Results
{
	public class CanceledResult : Result
	{
		public static ResultInstance<CanceledResult, IResult> Instance { get; }
			= new CanceledResult().AsResultInstance();

		protected CanceledResult() : base(false, "An operation was canceled.")
		{
		}
	}

	public class CommandNotFoundResult : Result
	{
		public static ResultInstance<CommandNotFoundResult, IResult> Instance { get; }
			= new CommandNotFoundResult().AsResultInstance();

		protected CommandNotFoundResult() : base(false, "Unable to find a matching command.")
		{
		}
	}

	public class ExceptionAfterCommandResult : Result
	{
		public static ResultInstance<ExceptionAfterCommandResult, IResult> Instance { get; }
			= new ExceptionAfterCommandResult().AsResultInstance();

		protected ExceptionAfterCommandResult() : base(false, "An exception occurred after a command was executed.")
		{
		}
	}

	public class ExceptionDuringCommandResult : Result
	{
		public static ResultInstance<ExceptionDuringCommandResult, IResult> Instance { get; }
			= new ExceptionDuringCommandResult().AsResultInstance();

		protected ExceptionDuringCommandResult() : base(false, "An exception occurred while a command was executing.")
		{
		}
	}

	public class InvalidContextResult : Result
	{
		public static ResultInstance<InvalidContextResult, IResult> Instance { get; }
			= new InvalidContextResult().AsResultInstance();

		protected InvalidContextResult() : base(false, "Invalid context type.")
		{
		}
	}

	public class InvalidParameterResult : Result
	{
		public static ResultInstance<InvalidParameterResult, IResult> Instance { get; }
			= new InvalidParameterResult().AsResultInstance();

		protected InvalidParameterResult() : base(false, "Invalid parameter type.")
		{
		}
	}

	public class MultiMatchHandlingErrorResult : Result
	{
		public static ResultInstance<MultiMatchHandlingErrorResult, IResult> Instance { get; }
			= new MultiMatchHandlingErrorResult().AsResultInstance();

		protected MultiMatchHandlingErrorResult() : base(false, "Multiple commands match.")
		{
		}
	}

	public class NotEnoughArgsResult : Result
	{
		public static ResultInstance<NotEnoughArgsResult, IResult> Instance { get; }
			= new NotEnoughArgsResult().AsResultInstance();

		protected NotEnoughArgsResult() : base(false, "Not enough arguments provided.")
		{
		}
	}

	public class NullParameterResult : Result
	{
		public static ResultInstance<NullParameterResult, IResult> Instance { get; }
			= new NullParameterResult().AsResultInstance();

		protected NullParameterResult() : base(false, "Parameter is null.")
		{
		}
	}

	public class QuoteMismatchResult : Result
	{
		public static ResultInstance<QuoteMismatchResult, IResult> Instance { get; }
			= new QuoteMismatchResult().AsResultInstance();

		protected QuoteMismatchResult() : base(false, "Unable to parse arguments: quote mismatch.")
		{
		}
	}

	public class SuccessResult : Result
	{
		public static ResultInstance<SuccessResult, IResult> Instance { get; }
			= new SuccessResult().AsResultInstance();

		protected SuccessResult() : base(true, "")
		{
		}
	}

	public class TimedOutResult : Result
	{
		public static ResultInstance<TimedOutResult, IResult> Instance { get; }
			= new TimedOutResult().AsResultInstance();

		protected TimedOutResult() : base(false, "An operation timed out.")
		{
		}
	}

	public class TooManyArgsResult : Result
	{
		public static ResultInstance<TooManyArgsResult, IResult> Instance { get; }
			= new TooManyArgsResult().AsResultInstance();

		protected TooManyArgsResult() : base(false, "Too many arguments provided.")
		{
		}
	}
}