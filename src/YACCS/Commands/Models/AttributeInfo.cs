using System;
using System.Reflection;

namespace YACCS.Commands.Models;

/// <summary>
/// Holds an attribute and information about its source.
/// </summary>
/// <param name="Source">
/// The source of this attribute. This can be <see cref="Type"/>,
/// <see cref="PropertyInfo"/>, <see cref="FieldInfo"/>, <see cref="PropertyInfo"/>,
/// or <see langword="null"/> if this is automatically generated.
/// </param>
/// <param name="Distance">
/// How far away from the command method declaration this attribute is.
/// </param>
/// <param name="Value">The attribute itself.</param>
public record class AttributeInfo(
	ICustomAttributeProvider? Source,
	int Distance,
	object Value
)
{
	/// <summary>
	/// The source of this attribute is the implementation of the method the command invokes.
	/// </summary>
	public const int ON_METHOD = 0;
	/// <summary>
	/// The source of this attribute is the definition of the method the command invokes.
	/// </summary>
	public const int ON_METHOD_INHERITED = 1;
	/// <summary>
	/// The source of this attribute is a class which directly or indirectly
	/// defines the method the command invokes.
	/// </summary>
	public const int ON_CLASS = 1000;
	/// <summary>
	/// This attribute is generated at runtime.
	/// </summary>
	public const int GENERATED = 10000;

	/// <inheritdoc cref="ON_METHOD" />
	public bool IsOnMethod => Distance == ON_METHOD;
	/// <inheritdoc cref="ON_METHOD_INHERITED" />
	public bool IsOnMethodInherited => Distance == ON_METHOD_INHERITED;
	/// <inheritdoc cref="ON_CLASS" />
	public bool IsOnClass => Distance is >= ON_CLASS and < GENERATED;
	/// <inheritdoc cref="GENERATED" />
	public bool IsGenerated => Distance >= GENERATED;

	/// <summary>
	/// Creates an instance of <see cref="AttributeInfo"/> and marks it as generated.
	/// </summary>
	/// <param name="value"></param>
	public AttributeInfo(object value) : this(null, GENERATED, value)
	{
	}
}