using System;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="ICategoryAttribute"/>
/// <summary>
/// Creates a new <see cref="CategoryAttribute"/>.
/// </summary>
/// <param name="category">
/// <inheritdoc cref="Category" path="/summary"/>
/// </param>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class CategoryAttribute(string category)
	: Attribute, ICategoryAttribute
{
	/// <inheritdoc />
	public virtual string Category { get; } = category;
}