#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using MorseCode.ITask;

using YACCS.Localization;
using YACCS.TypeReaders;

namespace YACCS.Results;

public static class CachedResults<T>
{
	public static CachedTypeReaderResult Canceled { get; }
		= new(CanceledResult.Instance);
	public static CachedTypeReaderResult DefaultSuccess { get; }
		= new(default(T)!);
	public static CachedTypeReaderResult InvalidContext { get; }
		= new(InvalidContextResult.Instance);
	public static CachedTypeReaderResult NamedArgBadCount { get; }
		= new(NamedArgBadCountResult.Instance);
	public static CachedTypeReaderResult ParseFailed { get; }
		= new(new ParseFailedResult(typeof(T)));
	public static CachedTypeReaderResult TimedOut { get; }
		= new(TimedOutResult.Instance);

	public class CachedTypeReaderResult
	{
		public ITypeReaderResult<T> Result { get; }
		public ITask<ITypeReaderResult<T>> Task { get; }

		public CachedTypeReaderResult(IResult result)
		{
			Result = TypeReaderResult<T>.FromError(result);
			Task = Result.AsITask();
		}

		public CachedTypeReaderResult(T value)
		{
			Result = TypeReaderResult<T>.FromSuccess(value);
			Task = Result.AsITask();
		}
	}
}

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

public class InteractionEndedResult : LocalizedResult
{
	public static InteractionEndedResult Instance { get; } = new();

	public InteractionEndedResult() : base(false, Keys.InteractionEndedResult)
	{
	}
}

public class InUseMustBeInUse : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public InUseMustBeInUse(Type type) : base(false, Keys.InUseMustBeInUse)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
}

public class InUseMustNotBeInUse : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public InUseMustNotBeInUse(Type type) : base(false, Keys.InUseMustNotBeInUse)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
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

public class NamedArgDuplicateResult : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgDuplicateResult(string name) : base(false, Keys.NamedArgDuplicateResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Name);
}

public class NamedArgInvalidDictionaryResult : LocalizedResult
{
	public static NamedArgInvalidDictionaryResult Instance { get; } = new();

	public NamedArgInvalidDictionaryResult() : base(false, Keys.NamedArgInvalidDictionaryResult)
	{
	}
}

public class NamedArgMissingValueResult : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgMissingValueResult(string name) : base(false, Keys.NamedArgMissingValueResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Name);
}

public class NamedArgNonExistentResult : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgNonExistentResult(string name) : base(false, Keys.NamedArgNonExistentResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
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

public class ParseFailedResult : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public ParseFailedResult(Type type) : base(false, Keys.ParseFailedResult)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
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
