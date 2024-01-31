namespace YACCS.Localization;

/// <summary>
/// Localizes a string or displays a fallback.
/// </summary>
/// <remarks>
/// Creates a new <see cref="NeedsLocalization"/>.
/// </remarks>
/// <param name="key">
/// <inheritdoc cref="Key" path="/summary"/>
/// </param>
/// <param name="fallback">
/// <inheritdoc cref="Fallback" path="/summary"/>
/// </param>
public readonly struct NeedsLocalization(string key, string? fallback = null)
{
	/// <summary>
	/// The value to use when <see cref="Key"/> is not registered
	/// in <see cref="Localize.Instance"/>.
	/// </summary>
	public string? Fallback { get; } = fallback;
	/// <summary>
	/// The key to search for in <see cref="Localize.Instance"/>.
	/// </summary>
	public string Key { get; } = key;
	/// <summary>
	/// Calls <see cref="Localize.This(string, string?)"/>.
	/// </summary>
	public string Localized => Localize.This(Key, Fallback);

	/// <inheritdoc />
	public override string ToString()
		=> Localized;
}