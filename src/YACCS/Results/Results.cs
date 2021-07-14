using System;

using YACCS.Localization;

namespace YACCS.Results
{
	public class CanceledResult : LocalizedResult
	{
		public static CanceledResult Instance { get; } = new();

		public CanceledResult() : base(false, Keys.CanceledResult)
		{
		}
	}

	public class CommandNotFoundResult : LocalizedResult
	{
		public static CommandNotFoundResult Instance { get; } = new();

		public CommandNotFoundResult() : base(false, Keys.CommandNotFoundResult)
		{
		}
	}

	public class ExceptionAfterCommandResult : LocalizedResult
	{
		public static ExceptionAfterCommandResult Instance { get; } = new();

		public ExceptionAfterCommandResult() : base(false, Keys.ExceptionAfterCommandResult)
		{
		}
	}

	public class ExceptionDuringCommandResult : LocalizedResult
	{
		public static ExceptionDuringCommandResult Instance { get; } = new();

		public ExceptionDuringCommandResult() : base(false, Keys.ExceptionDuringCommandResult)
		{
		}
	}

	public class FailureResult : Result
	{
		public static FailureResult Instance { get; } = new();

		public FailureResult() : this(string.Empty)
		{
		}

		public FailureResult(string message) : base(false, message)
		{
		}
	}

	public class InvalidContextResult : LocalizedResult
	{
		public static InvalidContextResult Instance { get; } = new();

		public InvalidContextResult() : base(false, Keys.InvalidContextResult)
		{
		}
	}

	public class InvalidParameterResult : LocalizedResult
	{
		public static InvalidParameterResult Instance { get; } = new();

		public InvalidParameterResult() : base(false, Keys.InvalidParameterResult)
		{
		}
	}

	public class MultiMatchHandlingErrorResult : LocalizedResult
	{
		public static MultiMatchHandlingErrorResult Instance { get; } = new();

		public MultiMatchHandlingErrorResult() : base(false, Keys.MultiMatchHandlingErrorResult)
		{
		}
	}

	public class NamedArgBadCountResult : LocalizedResult
	{
		public static NamedArgBadCountResult Instance { get; } = new();

		public NamedArgBadCountResult() : base(false, Keys.NamedArgBadCountResult)
		{
		}
	}

	public class NamedArgDuplicateResult : LocalizedResult
	{
		public string Name { get; }
		public override string Response => string.Format(base.Response, Name);

		public NamedArgDuplicateResult(string name) : base(false, Keys.NamedArgDuplicateResult)
		{
			Name = name;
		}

		public override string ToString(string ignored, IFormatProvider formatProvider)
			=> string.Format(formatProvider, base.Response, Name);
	}

	public class NamedArgMissingValueResult : LocalizedResult
	{
		public string Name { get; }
		public override string Response => string.Format(base.Response, Name);

		public NamedArgMissingValueResult(string name) : base(false, Keys.NamedArgMissingValueResult)
		{
			Name = name;
		}

		public override string ToString(string ignored, IFormatProvider formatProvider)
			=> string.Format(formatProvider, base.Response, Name);
	}

	public class NamedArgNonExistentResult : LocalizedResult
	{
		public string Name { get; }
		public override string Response => string.Format(base.Response, Name);

		public NamedArgNonExistentResult(string name) : base(false, Keys.NamedArgNonExistentResult)
		{
			Name = name;
		}

		public override string ToString(string ignored, IFormatProvider formatProvider)
			=> string.Format(formatProvider, base.Response, Name);
	}

	public class NotEnoughArgsResult : LocalizedResult
	{
		public static NotEnoughArgsResult Instance { get; } = new();

		public NotEnoughArgsResult() : base(false, Keys.NotEnoughArgsResult)
		{
		}
	}

	public class NullParameterResult : LocalizedResult
	{
		public static NullParameterResult Instance { get; } = new();

		public NullParameterResult() : base(false, Keys.NullParameterResult)
		{
		}
	}

	public class ParseFailedResult : LocalizedResult
	{
		public Type Type { get; }
		public override string Response => string.Format(base.Response, Type);

		protected ParseFailedResult(Type type) : base(false, Keys.ParseFailedResult)
		{
			Type = type;
		}

		public override string ToString(string ignored, IFormatProvider formatProvider)
			=> string.Format(formatProvider, base.Response, Type);
	}

	public class ParseFailedResult<T> : ParseFailedResult
	{
		public static ParseFailedResult<T> Instance { get; } = new();

		public ParseFailedResult() : base(typeof(T))
		{
		}
	}

	public class QuoteMismatchResult : LocalizedResult
	{
		public static QuoteMismatchResult Instance { get; } = new();

		public QuoteMismatchResult() : base(false, Keys.QuoteMismatchResult)
		{
		}
	}

	public class SuccessResult : Result
	{
		public static SuccessResult Instance { get; } = new();

		public SuccessResult() : this(string.Empty)
		{
		}

		public SuccessResult(string message) : base(true, message)
		{
		}
	}

	public class TimedOutResult : LocalizedResult
	{
		public static TimedOutResult Instance { get; } = new();

		public TimedOutResult() : base(false, Keys.TimedOutResult)
		{
		}
	}

	public class TooManyArgsResult : LocalizedResult
	{
		public static TooManyArgsResult Instance { get; } = new();

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