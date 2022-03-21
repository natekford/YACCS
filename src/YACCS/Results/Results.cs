#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using MorseCode.ITask;

using YACCS.Localization;
using YACCS.TypeReaders;

namespace YACCS.Results;

public static class CachedResults<T>
{
	public static CachedTypeReaderResult Canceled { get; }
		= new(Results.Canceled.Instance);
	public static CachedTypeReaderResult DefaultSuccess { get; }
		= new(default(T)!);
	public static CachedTypeReaderResult InvalidContext { get; }
		= new(Results.InvalidContext.Instance);
	public static CachedTypeReaderResult NamedArgBadCount { get; }
		= new(Results.NamedArgBadCount.Instance);
	public static CachedTypeReaderResult ParseFailed { get; }
		= new(new ParseFailed(typeof(T)));
	public static CachedTypeReaderResult TimedOut { get; }
		= new(Results.TimedOut.Instance);

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

public class Canceled : LocalizedResult
{
	public static Canceled Instance { get; } = new();

	public Canceled() : base(false, Keys.CanceledResult)
	{
	}
}

public class CommandNotFound : LocalizedResult
{
	public static CommandNotFound Instance { get; } = new();

	public CommandNotFound() : base(false, Keys.CommandNotFoundResult)
	{
	}
}

public class ExceptionAfterCommand : LocalizedResult
{
	public static ExceptionAfterCommand Instance { get; } = new();

	public ExceptionAfterCommand() : base(false, Keys.ExceptionAfterCommandResult)
	{
	}
}

public class ExceptionDuringCommand : LocalizedResult
{
	public static ExceptionDuringCommand Instance { get; } = new();

	public ExceptionDuringCommand() : base(false, Keys.ExceptionDuringCommandResult)
	{
	}
}

public class Failure : Result
{
	public static Failure Instance { get; } = new();

	public Failure() : this(string.Empty)
	{
	}

	public Failure(string message) : base(false, message)
	{
	}
}

public class InteractionEnded : LocalizedResult
{
	public static InteractionEnded Instance { get; } = new();

	public InteractionEnded() : base(false, Keys.InteractionEndedResult)
	{
	}
}

public class MustBeLocked : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public MustBeLocked(Type type) : base(false, Keys.MustBeLocked)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
}

public class MustBeUnlocked : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public MustBeUnlocked(Type type) : base(false, Keys.MustBeUnlocked)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
}

public class InvalidContext : LocalizedResult
{
	public static InvalidContext Instance { get; } = new();

	public InvalidContext() : base(false, Keys.InvalidContextResult)
	{
	}
}

public class InvalidParameter : LocalizedResult
{
	public static InvalidParameter Instance { get; } = new();

	public InvalidParameter() : base(false, Keys.InvalidParameterResult)
	{
	}
}

public class MultiMatchHandlingError : LocalizedResult
{
	public static MultiMatchHandlingError Instance { get; } = new();

	public MultiMatchHandlingError() : base(false, Keys.MultiMatchHandlingErrorResult)
	{
	}
}

public class NamedArgBadCount : LocalizedResult
{
	public static NamedArgBadCount Instance { get; } = new();

	public NamedArgBadCount() : base(false, Keys.NamedArgBadCountResult)
	{
	}
}

public class NamedArgDuplicate : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgDuplicate(string name) : base(false, Keys.NamedArgDuplicateResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Name);
}

public class NamedArgInvalidDictionary : LocalizedResult
{
	public static NamedArgInvalidDictionary Instance { get; } = new();

	public NamedArgInvalidDictionary() : base(false, Keys.NamedArgInvalidDictionaryResult)
	{
	}
}

public class NamedArgMissingValue : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgMissingValue(string name) : base(false, Keys.NamedArgMissingValueResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Name);
}

public class NamedArgNonExistent : LocalizedResult, IFormattable
{
	public string Name { get; }
	public override string Response => ToString(null, null);

	public NamedArgNonExistent(string name) : base(false, Keys.NamedArgNonExistentResult)
	{
		Name = name;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Name);
}

public class NotEnoughArgs : LocalizedResult
{
	public static NotEnoughArgs Instance { get; } = new();

	public NotEnoughArgs() : base(false, Keys.NotEnoughArgsResult)
	{
	}
}

public class NullParameter : LocalizedResult
{
	public static NullParameter Instance { get; } = new();

	public NullParameter() : base(false, Keys.NullParameterResult)
	{
	}
}

public class ParseFailed : LocalizedResult, IFormattable
{
	public override string Response => ToString(null, null);
	public Type Type { get; }

	public ParseFailed(Type type) : base(false, Keys.ParseFailedResult)
	{
		Type = type;
	}

	public string ToString(string? ignored, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, base.Response, Type);
}

public class QuoteMismatch : LocalizedResult
{
	public static QuoteMismatch Instance { get; } = new();

	public QuoteMismatch() : base(false, Keys.QuoteMismatchResult)
	{
	}
}

public class Success : Result
{
	public static Success Instance { get; } = new();

	public Success() : this(string.Empty)
	{
	}

	public Success(string message) : base(true, message)
	{
	}
}

public class TimedOut : LocalizedResult
{
	public static TimedOut Instance { get; } = new();

	public TimedOut() : base(false, Keys.TimedOutResult)
	{
	}
}

public class TooManyArgs : LocalizedResult
{
	public static TooManyArgs Instance { get; } = new();

	public TooManyArgs() : base(false, Keys.TooManyArgsResult)
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