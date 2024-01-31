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
/// <remarks>
/// Creates a new <see cref="MustBeGreaterThan"/>.
/// </remarks>
/// <param name="min">
/// <inheritdoc cref="Min" path="/summary"/>
/// </param>
public sealed class MustBeGreaterThan(int min)
	: FormattableLocalizedResult(false, Keys.MustBeGreaterThan)
{
	/// <summary>
	/// The minimum accepted value (inclusive).
	/// </summary>
	public int Min { get; } = min;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Min);
}

/// <summary>
/// The supplied integer argument was greater than the maximum accepted value (inclusive).
/// </summary>
/// <remarks>
/// Creates a new <see cref="MustBeLessThan"/>.
/// </remarks>
/// <param name="max">
/// <inheritdoc cref="Max" path="/summary"/>
/// </param>
public sealed class MustBeLessThan(int max)
	: FormattableLocalizedResult(false, Keys.MustBeLessThan)
{
	/// <summary>
	/// The maximum accepted value (inclusive).
	/// </summary>
	public int Max { get; } = max;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Max);
}

/// <summary>
/// The supplied argument wasn't in use while the command required it to be in use.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MustBeLocked"/>.
/// </remarks>
/// <param name="type">
/// <inheritdoc cref="Type" path="/summary"/>
/// </param>
public sealed class MustBeLocked(Type type)
	: FormattableLocalizedResult(false, Keys.MustBeLocked)
{
	/// <summary>
	/// The type of item being checked for if it's being used.
	/// </summary>
	public Type Type { get; } = type;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Type);
}

/// <summary>
/// The supplied argument was in use while the command required it to not be in use.
/// </summary>
/// <remarks>
/// Creates a new <see cref="MustBeUnlocked"/>.
/// </remarks>
/// <param name="type">
/// <inheritdoc cref="Type" path="/summary"/>
/// </param>
public sealed class MustBeUnlocked(Type type)
	: FormattableLocalizedResult(false, Keys.MustBeUnlocked)
{
	/// <summary>
	/// The type of item being checked for if it's being unused.
	/// </summary>
	public Type Type { get; } = type;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Type);
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
/// <remarks>
/// Creates a new <see cref="NamedArgDuplicate"/>.
/// </remarks>
/// <param name="name">
/// <inheritdoc cref="Name" path="/summary"/>
/// </param>
public sealed class NamedArgDuplicate(string name)
	: FormattableLocalizedResult(false, Keys.NamedArgDuplicateResult)
{
	/// <summary>
	/// The name of the argument that was provided multiple of.
	/// </summary>
	public string Name { get; } = name;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Name);
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
/// <remarks>
/// Creates a new <see cref="NamedArgMissingValue"/>.
/// </remarks>
/// <param name="name">
/// <inheritdoc cref="Name" path="/summary"/>
/// </param>
public sealed class NamedArgMissingValue(string name)
	: FormattableLocalizedResult(false, Keys.NamedArgMissingValueResult)
{
	/// <summary>
	/// The name of the argument that was missing a value.
	/// </summary>
	public string Name { get; } = name;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Name);
}

/// <summary>
/// There were named arguments provided that do not exist on the class being instantiated.
/// </summary>
/// <remarks>
/// Creates a new <see cref="NamedArgNonExistent"/>.
/// </remarks>
/// <param name="name">
/// <inheritdoc cref="Name" path="/summary"/>
/// </param>
public sealed class NamedArgNonExistent(string name)
	: FormattableLocalizedResult(false, Keys.NamedArgNonExistentResult)
{
	/// <summary>
	/// The name of the argument that wasn't found.
	/// </summary>
	public string Name { get; } = name;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Name);
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
/// <remarks>
/// Creates a new <see cref="ParseFailed"/>.
/// </remarks>
/// <param name="type">
/// <inheritdoc cref="Type" path="/summary"/>
/// </param>
public sealed class ParseFailed(Type type)
	: FormattableLocalizedResult(false, Keys.ParseFailedResult)
{
	/// <summary>
	/// The type that was failed to be parsed.
	/// </summary>
	public Type Type { get; } = type;

	/// <inheritdoc />
	public override string ToString(string? _, IFormatProvider? formatProvider)
		=> string.Format(formatProvider, Format, Type);
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