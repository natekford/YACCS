using YACCS.Commands.Attributes;

namespace YACCS.Localization;

/// <inheritdoc />
/// <summary>
/// Creates a new <see cref="LocalizedCategoryAttribute"/>.
/// </summary>
/// <param name="key">
/// <inheritdoc cref="Key" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public class LocalizedCategoryAttribute(string key)
	: CategoryAttribute(key)
{
	/// <inheritdoc />
	public override string Category => Localize.This(Key);
	/// <summary>
	/// The key for localization.
	/// </summary>
	public string Key { get; } = key;
}