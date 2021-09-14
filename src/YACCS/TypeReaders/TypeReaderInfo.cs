namespace YACCS.TypeReaders
{
	/// <summary>
	/// Contains a type reader and the types it targets.
	/// </summary>
	public readonly struct TypeReaderInfo
	{
		/// <summary>
		/// The type reader.
		/// </summary>
		public ITypeReader Instance { get; }
		/// <summary>
		/// Whether or not the type reader should override already existing ones when added.
		/// </summary>
		public bool OverrideExistingTypeReaders { get; }
		/// <summary>
		/// The types the type reader will be added for.
		/// </summary>
		public IReadOnlyList<Type> TargetTypes { get; }

		/// <summary>
		/// Creates a new <see cref="TypeReaderInfo"/>.
		/// </summary>
		/// <param name="targetTypes">
		/// <inheritdoc cref="TargetTypes" path="/summary"/>
		/// </param>
		/// <param name="overrideExistingTypeReaders">
		/// <inheritdoc cref="OverrideExistingTypeReaders" path="/summary"/>
		/// </param>
		/// <param name="instance">
		/// <inheritdoc cref="Instance" path="/summary"/>
		/// </param>
		public TypeReaderInfo(
			IReadOnlyList<Type> targetTypes,
			bool overrideExistingTypeReaders,
			ITypeReader instance)
		{
			Instance = instance;
			OverrideExistingTypeReaders = overrideExistingTypeReaders;
			TargetTypes = targetTypes;
		}
	}
}