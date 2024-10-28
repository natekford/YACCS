using System;

using YACCS.Commands.Attributes;

namespace YACCS.Localization;

/// <inheritdoc/>
/// <summary>
/// Creates a new <see cref="LocalizedNameAttribute"/>.
/// </summary>
/// <param name="key">
/// <inheritdoc cref="Key" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class LocalizedNameAttribute(string key)
	: NameAttribute(key)
{
	/// <summary>
	/// The key for localization.
	/// </summary>
	public string Key { get; } = key;
	/// <inheritdoc/>
	public override string Name => Localize.This(Key);
}