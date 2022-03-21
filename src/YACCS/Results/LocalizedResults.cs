using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// Something was canceled.
/// </summary>
public sealed class Canceled : SingletonLocalizedResult<Canceled>
{
	/// <summary>
	/// Creates a new <see cref="Canceled"/>.
	/// </summary>
	public Canceled() : base(false, Keys.CanceledResult)
	{
	}
}

/// <summary>
/// A command was unable to be found.
/// </summary>
public sealed class CommandNotFound : SingletonLocalizedResult<CommandNotFound>
{
	/// <summary>
	/// Creates a new <see cref="CommandNotFound"/>.
	/// </summary>
	public CommandNotFound() : base(false, Keys.CommandNotFoundResult)
	{
	}
}

/// <summary>
/// An exception occurred after a command was executed.
/// </summary>
public sealed class ExceptionAfterCommand : SingletonLocalizedResult<ExceptionAfterCommand>
{
	/// <summary>
	/// Creates a new <see cref="ExceptionAfterCommand"/>.
	/// </summary>
	public ExceptionAfterCommand() : base(false, Keys.ExceptionAfterCommandResult)
	{
	}
}

/// <summary>
/// An exception occurred while a command was being executed.
/// </summary>
public sealed class ExceptionDuringCommand : SingletonLocalizedResult<ExceptionDuringCommand>
{
	/// <summary>
	/// Creates a new <see cref="ExceptionDuringCommand"/>.
	/// </summary>
	public ExceptionDuringCommand() : base(false, Keys.ExceptionDuringCommandResult)
	{
	}
}

/// <summary>
/// An interaction ended (not canceled or timed out).
/// </summary>
public sealed class InteractionEnded : SingletonLocalizedResult<InteractionEnded>
{
	/// <summary>
	/// Creates a new <see cref="InteractionEnded"/>.
	/// </summary>
	public InteractionEnded() : base(false, Keys.InteractionEndedResult)
	{
	}
}

/// <summary>
/// The supplied context is not valid for the command attempting to be executed.
/// </summary>
public sealed class InvalidContext : SingletonLocalizedResult<InvalidContext>
{
	/// <summary>
	/// Creates a new <see cref="InvalidContext"/>.
	/// </summary>
	public InvalidContext() : base(false, Keys.InvalidContextResult)
	{
	}
}

/// <summary>
/// The passed in parameter is not the correct type for the parameter precondition
/// receiving it.
/// </summary>
public sealed class InvalidParameter : SingletonLocalizedResult<InvalidParameter>
{
	/// <summary>
	/// Creates a new <see cref="InvalidParameter"/>.
	/// </summary>
	public InvalidParameter() : base(false, Keys.InvalidParameterResult)
	{
	}
}

/// <summary>
/// Multiple commands matched the passed in arguments and the command service was configured
/// to treat this as an error.
/// </summary>
public sealed class MultiMatchHandlingError : SingletonLocalizedResult<MultiMatchHandlingError>
{
	/// <summary>
	/// Creates a new <see cref="MultiMatchHandlingError"/>.
	/// </summary>
	public MultiMatchHandlingError() : base(false, Keys.MultiMatchHandlingErrorResult)
	{
	}
}

/// <summary>
/// The supplied integer argument was less than the minimum accepted value (inclusive).
/// </summary>
public sealed class MustBeGreaterThan : FormattableLocalizedResult
{
	/// <summary>
	/// The minimum accepted value (inclusive).
	/// </summary>
	public int Min { get; }

	/// <summary>
	/// Creates a new <see cref="MustBeGreaterThan"/>.
	/// </summary>
	/// <param name="min">
	/// <inheritdoc cref="Min" path="/summary"/>
	/// </param>
	public MustBeGreaterThan(int min) : base(false, Keys.MustBeGreaterThan)
	{
		Min = min;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Min);
}

/// <summary>
/// The supplied integer argument was greater than the maximum accepted value (inclusive).
/// </summary>
public sealed class MustBeLessThan : FormattableLocalizedResult
{
	/// <summary>
	/// The maximum accepted value (inclusive).
	/// </summary>
	public int Max { get; }

	/// <summary>
	/// Creates a new <see cref="MustBeLessThan"/>.
	/// </summary>
	/// <param name="max">
	/// <inheritdoc cref="Max" path="/summary"/>
	/// </param>
	public MustBeLessThan(int max) : base(false, Keys.MustBeLessThan)
	{
		Max = max;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Max);
}

/// <summary>
/// The supplied argument wasn't in use while the command required it to be in use.
/// </summary>
public sealed class MustBeLocked : FormattableLocalizedResult
{
	/// <summary>
	/// The type of item being checked for if it's being used.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Creates a new <see cref="MustBeLocked"/>.
	/// </summary>
	/// <param name="type">
	/// <inheritdoc cref="Type" path="/summary"/>
	/// </param>
	public MustBeLocked(Type type) : base(false, Keys.MustBeLocked)
	{
		Type = type;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Type);
}

/// <summary>
/// The supplied argument was in use while the command required it to not be in use.
/// </summary>
public sealed class MustBeUnlocked : FormattableLocalizedResult
{
	/// <summary>
	/// The type of item being checked for if it's being unused.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Creates a new <see cref="MustBeUnlocked"/>.
	/// </summary>
	/// <param name="type">
	/// <inheritdoc cref="Type" path="/summary"/>
	/// </param>
	public MustBeUnlocked(Type type) : base(false, Keys.MustBeUnlocked)
	{
		Type = type;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Type);
}

/// <summary>
/// There was an odd number of arguments supplied to a named argument parameter.
/// </summary>
public sealed class NamedArgBadCount : SingletonLocalizedResult<NamedArgBadCount>
{
	/// <summary>
	/// Creates a new <see cref="NamedArgBadCount"/>.
	/// </summary>
	public NamedArgBadCount() : base(false, Keys.NamedArgBadCountResult)
	{
	}
}

/// <summary>
/// There were multiple values provided with the same argument name.
/// </summary>
public sealed class NamedArgDuplicate : FormattableLocalizedResult
{
	/// <summary>
	/// The name of the argument that was provided multiple of.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Creates a new <see cref="NamedArgDuplicate"/>.
	/// </summary>
	/// <param name="name">
	/// <inheritdoc cref="Name" path="/summary"/>
	/// </param>
	public NamedArgDuplicate(string name) : base(false, Keys.NamedArgDuplicateResult)
	{
		Name = name;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Name);
}

/// <summary>
/// The passed in dictionary for named argument command execution is an invalid type.
/// </summary>
public sealed class NamedArgInvalidDictionary : SingletonLocalizedResult<NamedArgInvalidDictionary>
{
	/// <summary>
	/// Creates a new <see cref="NamedArgInvalidDictionary"/>.
	/// </summary>
	public NamedArgInvalidDictionary() : base(false, Keys.NamedArgInvalidDictionaryResult)
	{
	}
}

/// <summary>
/// A required named argument did not have a value set.
/// </summary>
public sealed class NamedArgMissingValue : FormattableLocalizedResult
{
	/// <summary>
	/// The name of the argument that was missing a value.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Creates a new <see cref="NamedArgMissingValue"/>.
	/// </summary>
	/// <param name="name">
	/// <inheritdoc cref="Name" path="/summary"/>
	/// </param>
	public NamedArgMissingValue(string name) : base(false, Keys.NamedArgMissingValueResult)
	{
		Name = name;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Name);
}

/// <summary>
/// There were named arguments provided that do not exist on the class being instantiated.
/// </summary>
public sealed class NamedArgNonExistent : FormattableLocalizedResult
{
	/// <summary>
	/// The name of the argument that wasn't found.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Creates a new <see cref="NamedArgNonExistent"/>.
	/// </summary>
	/// <param name="name">
	/// <inheritdoc cref="Name" path="/summary"/>
	/// </param>
	public NamedArgNonExistent(string name) : base(false, Keys.NamedArgNonExistentResult)
	{
		Name = name;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Name);
}

/// <summary>
/// Not enough arguments were provided to the command.
/// </summary>
public sealed class NotEnoughArgs : SingletonLocalizedResult<NotEnoughArgs>
{
	/// <summary>
	/// Creates a new <see cref="NotEnoughArgs"/>.
	/// </summary>
	public NotEnoughArgs() : base(false, Keys.NotEnoughArgsResult)
	{
	}
}

/// <summary>
/// A parameter was null when it should not have been.
/// </summary>
public sealed class NullParameter : SingletonLocalizedResult<NullParameter>
{
	/// <summary>
	/// Creates a new <see cref="NullParameter"/>.
	/// </summary>
	public NullParameter() : base(false, Keys.NullParameterResult)
	{
	}
}

/// <summary>
/// Failed to parse an item of type <see cref="Type"/>.
/// </summary>
public sealed class ParseFailed : FormattableLocalizedResult
{
	/// <summary>
	/// The type that was failed to be parsed.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Creates a new <see cref="ParseFailed"/>.
	/// </summary>
	/// <param name="type">
	/// <inheritdoc cref="Type" path="/summary"/>
	/// </param>
	public ParseFailed(Type type) : base(false, Keys.ParseFailedResult)
	{
		Type = type;
	}

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Response, Type);
}

/// <summary>
/// Provided string had quotes that were unable to be parsed.
/// </summary>
public sealed class QuoteMismatch : SingletonLocalizedResult<QuoteMismatch>
{
	/// <summary>
	/// Creates a new <see cref="QuoteMismatch"/>.
	/// </summary>
	public QuoteMismatch() : base(false, Keys.QuoteMismatchResult)
	{
	}
}

/// <summary>
/// A timed function finished without success or cancellation.
/// </summary>
public sealed class TimedOut : SingletonLocalizedResult<TimedOut>
{
	/// <summary>
	/// Creates a new <see cref="TimedOut"/>.
	/// </summary>
	public TimedOut() : base(false, Keys.TimedOutResult)
	{
	}
}

/// <summary>
/// Too many arguments were provided to the command.
/// </summary>
public sealed class TooManyArgs : SingletonLocalizedResult<TooManyArgs>
{
	/// <summary>
	/// Creates a new <see cref="TooManyArgs"/>.
	/// </summary>
	public TooManyArgs() : base(false, Keys.TooManyArgsResult)
	{
	}
}