using YACCS.Localization;

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
	public static LocalizedResult<int> MustBeGreaterThan(int min)
		=> new(false, Keys.MustBeGreaterThan, min);

	/// <summary>
	/// The supplied integer argument was greater than the maximum accepted value (inclusive).
	/// </summary>
	/// <param name="max">The maximum accepted value (inclusive).</param>
	/// <returns></returns>
	public static LocalizedResult<int> MustBeLessThan(int max)
		=> new(false, Keys.MustBeLessThan, max);

	/// <summary>
	/// The supplied argument wasn't in use while the command required it to be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being used.</param>
	/// <returns></returns>
	public static LocalizedResult<Type> MustBeLocked(Type type)
		=> new(false, Keys.MustBeLocked, type);

	/// <summary>
	/// The supplied argument was in use while the command required it to not be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being unused.</param>
	/// <returns></returns>
	public static LocalizedResult<Type> MustBeUnlocked(Type type)
		=> new(false, Keys.MustBeUnlocked, type);

	/// <summary>
	/// There were multiple values provided with the same argument name.
	/// </summary>
	/// <param name="name">The name of the argument that was provided multiple of.</param>
	/// <returns></returns>
	public static LocalizedResult<string> NamedArgDuplicate(string name)
		=> new(false, Keys.NamedArgDuplicateResult, name);

	/// <summary>
	/// A required named argument did not have a value set.
	/// </summary>
	/// <param name="name">The name of the argument that was missing a value.</param>
	/// <returns></returns>
	public static LocalizedResult<string> NamedArgMissingValue(string name)
		=> new(false, Keys.NamedArgMissingValueResult, name);

	/// <summary>
	/// There were named arguments provided that do not exist on the class being instantiated.
	/// </summary>
	/// <param name="name">The name of the argument that wasn't found.</param>
	/// <returns></returns>
	public static LocalizedResult<string> NamedArgNonExistent(string name)
		=> new(false, Keys.NamedArgNonExistentResult, name);

	/// <summary>
	/// Failed to parse an item of type <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type that was failed to be parsed.</param>
	/// <returns></returns>
	public static LocalizedResult<Type> ParseFailed(Type type)
		=> new(false, Keys.ParseFailedResult, type);
}