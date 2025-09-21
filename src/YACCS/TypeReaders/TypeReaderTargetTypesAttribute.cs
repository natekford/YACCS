using System;
using System.Collections.Generic;

namespace YACCS.TypeReaders;

/// <summary>
/// Allows for specifying what types a type reader targets.
/// </summary>
/// <param name="types">
/// <inheritdoc cref="TargetTypes" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TypeReaderTargetTypesAttribute(params Type[] types) : Attribute
{
	/// <summary>
	/// Whether or not to override any existing type readers.
	/// </summary>
	public bool OverrideExistingTypeReaders { get; set; }
	/// <summary>
	/// The types to target with this type reader.
	/// </summary>
	public IReadOnlyList<Type> TargetTypes { get; } = types;
}