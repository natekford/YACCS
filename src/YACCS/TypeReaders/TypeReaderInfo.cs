using System;
using System.Collections.Generic;

namespace YACCS.TypeReaders;

/// <summary>
/// Contains a type reader and the types it targets.
/// </summary>
/// <remarks>
/// Creates a new <see cref="TypeReaderInfo"/>.
/// </remarks>
/// <param name="TargetTypes">
/// The types the type reader will be added for.
/// </param>
/// <param name="OverrideExistingTypeReaders">
/// Whether or not the type reader should override already existing ones when added.
/// </param>
/// <param name="Instance">
/// The type reader.
/// </param>
public record TypeReaderInfo(
	IReadOnlyList<Type> TargetTypes,
	bool OverrideExistingTypeReaders,
	ITypeReader Instance
);