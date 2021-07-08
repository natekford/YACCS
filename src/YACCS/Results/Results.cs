using System;

namespace YACCS.Results
{
	public class CanceledResult : ResultWithSingleton<CanceledResult, IResult>
	{
		public CanceledResult() : base(false, "An operation was canceled.")
		{
		}
	}

	public class CommandNotFoundResult : ResultWithSingleton<CommandNotFoundResult, IResult>
	{
		public CommandNotFoundResult() : base(false, "Unable to find a matching command.")
		{
		}
	}

	public class ExceptionAfterCommandResult : ResultWithSingleton<ExceptionAfterCommandResult, IResult>
	{
		public ExceptionAfterCommandResult() : base(false, "An exception occurred after a command was executed.")
		{
		}
	}

	public class ExceptionDuringCommandResult : ResultWithSingleton<ExceptionDuringCommandResult, IResult>
	{
		public ExceptionDuringCommandResult() : base(false, "An exception occurred while a command was executing.")
		{
		}
	}

	public class FailureResult : ResultWithSingleton<FailureResult, IResult>
	{
		public FailureResult() : this(string.Empty)
		{
		}

		public FailureResult(string message) : base(false, message)
		{
		}
	}

	public class InvalidContextResult : ResultWithSingleton<InvalidContextResult, IResult>
	{
		public InvalidContextResult() : base(false, "Invalid context type.")
		{
		}
	}

	public class InvalidParameterResult : ResultWithSingleton<InvalidParameterResult, IResult>
	{
		public InvalidParameterResult() : base(false, "Invalid parameter type.")
		{
		}
	}

	public class MultiMatchHandlingErrorResult : ResultWithSingleton<MultiMatchHandlingErrorResult, IResult>
	{
		public MultiMatchHandlingErrorResult() : base(false, "Multiple commands match.")
		{
		}
	}

	public class NamedArgBadCountResult : ResultWithSingleton<NamedArgBadCountResult, IResult>
	{
		public NamedArgBadCountResult() : base(false, "There is not an even number of arguments supplied.")
		{
		}
	}

	public class NamedArgDuplicateResult : Result
	{
		public string Name { get; }

		public NamedArgDuplicateResult(string name) : base(false, $"Duplicate value for named argument {name}.")
		{
			Name = name;
		}
	}

	public class NamedArgMissingValueResult : Result
	{
		public string Name { get; }

		public NamedArgMissingValueResult(string name) : base(false, $"Missing a value for named argument {name}.")
		{
			Name = name;
		}
	}

	public class NamedArgNonExistentResult : Result
	{
		public string Name { get; }

		public NamedArgNonExistentResult(string name) : base(false, $"Named argument does not exist {name}.")
		{
			Name = name;
		}
	}

	public class NotEnoughArgsResult : ResultWithSingleton<NotEnoughArgsResult, IResult>
	{
		public NotEnoughArgsResult() : base(false, "Not enough arguments provided.")
		{
		}
	}

	public class NullParameterResult : ResultWithSingleton<NullParameterResult, IResult>
	{
		public NullParameterResult() : base(false, "Parameter is null.")
		{
		}
	}

	public class ParseFailedResult : Result
	{
		public Type Type { get; }

		protected ParseFailedResult(Type type) : base(false, $"Failed to parse {type.Name}.")
		{
			Type = type;
		}
	}

	public class ParseFailedResult<T> : ParseFailedResult
	{
		public static ResultInstance<ParseFailedResult<T>, IResult> Instance { get; }
			= new(new());

		public ParseFailedResult() : base(typeof(T))
		{
		}
	}

	public class QuoteMismatchResult : ResultWithSingleton<QuoteMismatchResult, IResult>
	{
		public QuoteMismatchResult() : base(false, "Unable to parse arguments: quote mismatch.")
		{
		}
	}

	public class SuccessResult : ResultWithSingleton<SuccessResult, IResult>
	{
		public SuccessResult() : this(string.Empty)
		{
		}

		public SuccessResult(string message) : base(true, message)
		{
		}
	}

	public class TimedOutResult : ResultWithSingleton<TimedOutResult, IResult>
	{
		public TimedOutResult() : base(false, "An operation timed out.")
		{
		}
	}

	public class TooManyArgsResult : ResultWithSingleton<TooManyArgsResult, IResult>
	{
		public TooManyArgsResult() : base(false, "Too many arguments provided.")
		{
		}
	}

	public class ValueResult : Result
	{
		public object? Value { get; }

		public ValueResult(object? value) : base(true, value?.ToString() ?? string.Empty)
		{
			Value = value;
		}
	}
}