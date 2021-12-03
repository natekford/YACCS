using System.Collections.Immutable;

namespace YACCS.TypeReaders;

/// <summary>
/// Allows for specifying what types a type reader targets.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TypeReaderTargetTypesAttribute : Attribute
{
	/// <summary>
	/// Whether or not to override any existing type readers.
	/// </summary>
	public bool OverrideExistingTypeReaders { get; }
	/// <summary>
	/// The types to target with this type reader.
	/// </summary>
	public IReadOnlyList<Type> TargetTypes { get; }

	/// <summary>
	/// Creates a new <see cref="TypeReaderTargetTypesAttribute"/>.
	/// </summary>
	/// <param name="types">
	/// <inheritdoc cref="TargetTypes" path="/summary"/>
	/// </param>
	public TypeReaderTargetTypesAttribute(params Type[] types)
	{
		TargetTypes = types.ToImmutableArray();
	}
}
