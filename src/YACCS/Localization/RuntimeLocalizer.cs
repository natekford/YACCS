using System.Globalization;

namespace YACCS.Localization;

/// <summary>
/// Wrapper for a localized dictionary.
/// </summary>
public sealed class RuntimeLocalizer : ILocalizer
{
	/// <summary>
	/// Values specified at runtime.
	/// </summary>
	public Localized<Dictionary<string, string>> Overrides { get; }
		= Localized.Create<Dictionary<string, string>>();

	/// <inheritdoc />
	public string? Get(string key, CultureInfo? culture = null)
		=> Overrides[culture!].TryGetValue(key, out var value) ? value : null;
}
