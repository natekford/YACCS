using System;

using YACCS.Localization;

namespace YACCS.Results
{
	public class CanceledResult : ResultWithSingleton<CanceledResult, IResult>
	{
		public CanceledResult() : base(false, Keys.CanceledResult)
		{
		}
	}

	public class CommandNotFoundResult : ResultWithSingleton<CommandNotFoundResult, IResult>
	{
		public CommandNotFoundResult() : base(false, Keys.CommandNotFoundResult)
		{
		}
	}

	public class ExceptionAfterCommandResult : ResultWithSingleton<ExceptionAfterCommandResult, IResult>
	{
		public ExceptionAfterCommandResult() : base(false, Keys.ExceptionAfterCommandResult)
		{
		}
	}

	public class ExceptionDuringCommandResult : ResultWithSingleton<ExceptionDuringCommandResult, IResult>
	{
		public ExceptionDuringCommandResult() : base(false, Keys.ExceptionDuringCommandResult)
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
		public InvalidContextResult() : base(false, Keys.InvalidContextResult)
		{
		}
	}

	public class InvalidParameterResult : ResultWithSingleton<InvalidParameterResult, IResult>
	{
		public InvalidParameterResult() : base(false, Keys.InvalidParameterResult)
		{
		}
	}

	public class MultiMatchHandlingErrorResult : ResultWithSingleton<MultiMatchHandlingErrorResult, IResult>
	{
		public MultiMatchHandlingErrorResult() : base(false, Keys.MultiMatchHandlingErrorResult)
		{
		}
	}

	public class NamedArgBadCountResult : ResultWithSingleton<NamedArgBadCountResult, IResult>
	{
		public NamedArgBadCountResult() : base(false, Keys.NamedArgBadCountResult)
		{
		}
	}

	public class NamedArgDuplicateResult : Result
	{
		public string Name { get; }

		public NamedArgDuplicateResult(string name)
			: base(false, string.Format(Keys.NamedArgDuplicateResult, name))
		{
			Name = name;
		}
	}

	public class NamedArgMissingValueResult : Result
	{
		public string Name { get; }

		public NamedArgMissingValueResult(string name)
			: base(false, string.Format(Keys.NamedArgMissingValueResult, name))
		{
			Name = name;
		}
	}

	public class NamedArgNonExistentResult : Result
	{
		public string Name { get; }

		public NamedArgNonExistentResult(string name)
			: base(false, string.Format(Keys.NamedArgNonExistentResult, name))
		{
			Name = name;
		}
	}

	public class NotEnoughArgsResult : ResultWithSingleton<NotEnoughArgsResult, IResult>
	{
		public NotEnoughArgsResult() : base(false, Keys.NotEnoughArgsResult)
		{
		}
	}

	public class NullParameterResult : ResultWithSingleton<NullParameterResult, IResult>
	{
		public NullParameterResult() : base(false, Keys.NullParameterResult)
		{
		}
	}

	public class ParseFailedResult : Result
	{
		public Type Type { get; }

		protected ParseFailedResult(Type type)
			: base(false, string.Format(Keys.ParseFailedResult, type.Name))
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
		public QuoteMismatchResult() : base(false, Keys.QuoteMismatchResult)
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
		public TimedOutResult() : base(false, Keys.TimedOutResult)
		{
		}
	}

	public class TooManyArgsResult : ResultWithSingleton<TooManyArgsResult, IResult>
	{
		public TooManyArgsResult() : base(false, Keys.TooManyArgsResult)
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