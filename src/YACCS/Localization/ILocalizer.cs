using System.Globalization;

namespace YACCS.Localization;

/// <summary>
/// Defines a method for localizing a key.
/// </summary>
public interface ILocalizer
{
	/// <summary>
	/// Attempts to find a localized value for <paramref name="key"/>.
	/// </summary>
	/// <param name="key">The key to search for.</param>
	/// <param name="culture">The culture to search in.</param>
	/// <returns>
	/// A localized string, or <see langword="null"/> if one is not found.
	/// </returns>
	string? Get(string key, CultureInfo? culture = null);
}