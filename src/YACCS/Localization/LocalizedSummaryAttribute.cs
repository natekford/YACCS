using System;

using YACCS.Help.Attributes;

namespace YACCS.Localization;

/// <inheritdoc />
/// <summary>
/// Creates a new <see cref="LocalizedSummaryAttribute"/>.
/// </summary>
/// <param name="key">
/// <inheritdoc cref="Key" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class LocalizedSummaryAttribute(string key)
	: SummaryAttribute(key)
{
	/// <summary>
	/// The key for localization.
	/// </summary>
	public string Key { get; } = key;
	/// <inheritdoc />
	public override string Summary => Localize.This(Key);
}