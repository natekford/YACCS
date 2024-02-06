namespace YACCS.Results;

/// <summary>
/// Uncached results.
/// </summary>
public static class UncachedResults
{
	/// <summary>
	/// The supplied integer argument was less than the minimum accepted value (inclusive).
	/// </summary>
	/// <param name="min">The minimum accepted value (inclusive).</param>
	/// <returns></returns>
	public static MustBeGreaterThan MustBeGreaterThan(int min) => new(min);

	/// <summary>
	/// The supplied integer argument was greater than the maximum accepted value (inclusive).
	/// </summary>
	/// <param name="max">The maximum accepted value (inclusive).</param>
	/// <returns></returns>
	public static MustBeLessThan MustBeLessThan(int max) => new(max);

	/// <summary>
	/// The supplied argument wasn't in use while the command required it to be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being used.</param>
	/// <returns></returns>
	public static MustBeLocked MustBeLocked(Type type) => new(type);

	/// <summary>
	/// The supplied argument was in use while the command required it to not be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being unused.</param>
	/// <returns></returns>
	public static MustBeUnlocked MustBeUnlocked(Type type) => new(type);

	/// <summary>
	/// There were multiple values provided with the same argument name.
	/// </summary>
	/// <param name="name">The name of the argument that was provided multiple of.</param>
	/// <returns></returns>
	public static NamedArgDuplicate NamedArgDuplicate(string name) => new(name);

	/// <summary>
	/// A required named argument did not have a value set.
	/// </summary>
	/// <param name="name">The name of the argument that was missing a value.</param>
	/// <returns></returns>
	public static NamedArgMissingValue NamedArgMissingValue(string name) => new(name);

	/// <summary>
	/// There were named arguments provided that do not exist on the class being instantiated.
	/// </summary>
	/// <param name="name">The name of the argument that wasn't found.</param>
	/// <returns></returns>
	public static NamedArgNonExistent NamedArgNonExistent(string name) => new(name);

	/// <summary>
	/// Failed to parse an item of type <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type that was failed to be parsed.</param>
	/// <returns></returns>
	public static ParseFailed ParseFailed(Type type) => new(type);
}