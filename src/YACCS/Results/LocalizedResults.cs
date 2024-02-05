using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// The supplied integer argument was less than the minimum accepted value (inclusive).
/// </summary>
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
/// There were multiple values provided with the same argument name.
/// </summary>
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
/// A required named argument did not have a value set.
/// </summary>
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
/// Failed to parse an item of type <see cref="Type"/>.
/// </summary>
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