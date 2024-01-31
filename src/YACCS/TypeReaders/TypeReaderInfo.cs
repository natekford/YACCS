namespace YACCS.TypeReaders;

/// <summary>
/// Contains a type reader and the types it targets.
/// </summary>
/// <remarks>
/// Creates a new <see cref="TypeReaderInfo"/>.
/// </remarks>
/// <param name="targetTypes">
/// <inheritdoc cref="TargetTypes" path="/summary"/>
/// </param>
/// <param name="overrideExistingTypeReaders">
/// <inheritdoc cref="OverrideExistingTypeReaders" path="/summary"/>
/// </param>
/// <param name="instance">
/// <inheritdoc cref="Instance" path="/summary"/>
/// </param>
public readonly struct TypeReaderInfo(
	IReadOnlyList<Type> targetTypes,
	bool overrideExistingTypeReaders,
	ITypeReader instance)
{
	/// <summary>
	/// The type reader.
	/// </summary>
	public ITypeReader Instance { get; } = instance;
	/// <summary>
	/// Whether or not the type reader should override already existing ones when added.
	/// </summary>
	public bool OverrideExistingTypeReaders { get; } = overrideExistingTypeReaders;
	/// <summary>
	/// The types the type reader will be added for.
	/// </summary>
	public IReadOnlyList<Type> TargetTypes { get; } = targetTypes;
}