namespace YACCS.Commands.Attributes;

/// <summary>
/// Utilities for attributes.
/// </summary>
public static class AttributeUtils
{
	/// <summary>
	/// Specifies that this attribute can only target <see cref="AttributeTargets.Class"/>
	/// and <see cref="AttributeTargets.Method"/>.
	/// </summary>
	public const AttributeTargets COMMANDS = 0
		| AttributeTargets.Class
		| AttributeTargets.Method;

	/// <summary>
	/// Specifies that this attribute can only target <see cref="AttributeTargets.Parameter"/>,
	/// <see cref="AttributeTargets.Property"/>, and <see cref="AttributeTargets.Field"/>.
	/// </summary>
	public const AttributeTargets PARAMETERS = 0
		| AttributeTargets.Parameter
		| AttributeTargets.Property
		| AttributeTargets.Field;

	internal static TValue ThrowIfDuplicate<TAttribute, TValue>(
		this TAttribute attribute,
		Func<TAttribute, TValue> converter,
		ref int count)
	{
		if (count > 0)
		{
			var name = typeof(TAttribute).Name;
			throw new InvalidOperationException($"Duplicate {name} attribute.");
		}

		++count;
		return converter.Invoke(attribute);
	}
}