using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results;

/// <summary>
/// Utilities for results.
/// </summary>
public static class ResultUtils
{
	/// <summary>
	/// Recursively searches for a <see cref="ValueResult"/> from <paramref name="result"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="result">The result to get a value from.</param>
	/// <param name="value">The retrieved value.</param>
	/// <returns>A bool indicating success or failure.</returns>
	public static bool TryGetValue<T>(
		this IResult result,
		[NotNullWhen(true)] out T value)
	{
		// Recursion is less complicated than a do while loop for this
		if (result is ValueResult vResult && vResult.Value is T t)
		{
			value = t;
			return true;
		}
		if (result is INestedResult nResult)
		{
			return nResult.InnerResult.TryGetValue(out value);
		}
		value = default!;
		return false;
	}
}