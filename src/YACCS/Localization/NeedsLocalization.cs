﻿namespace YACCS.Localization;

/// <summary>
/// Localizes a string or displays a fallback.
/// </summary>
public readonly struct NeedsLocalization
{
	/// <summary>
	/// The value to use when <see cref="Key"/> is not registered
	/// in <see cref="Localize.Instance"/>.
	/// </summary>
	public string? Fallback { get; }
	/// <summary>
	/// The key to search for in <see cref="Localize.Instance"/>.
	/// </summary>
	public string Key { get; }
	/// <summary>
	/// Calls <see cref="Localize.This(string, string?)"/>.
	/// </summary>
	public string Localized => Localize.This(Key, Fallback);

	/// <summary>
	/// Creates a new <see cref="NeedsLocalization"/>.
	/// </summary>
	/// <param name="key">
	/// <inheritdoc cref="Key" path="/summary"/>
	/// </param>
	/// <param name="fallback">
	/// <inheritdoc cref="Fallback" path="/summary"/>
	/// </param>
	public NeedsLocalization(string key, string? fallback = null)
	{
		Key = key;
		Fallback = fallback;
	}

	/// <inheritdoc />
	public override string ToString()
		=> Localized;
}