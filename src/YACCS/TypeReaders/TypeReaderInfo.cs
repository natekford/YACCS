using System;
using System.Collections.Generic;

namespace YACCS.TypeReaders
{
	public readonly struct TypeReaderInfo
	{
		public ITypeReader Instance { get; }
		public bool OverrideExistingTypeReaders { get; }
		public IReadOnlyList<Type> TargetTypes { get; }

		public TypeReaderInfo(
			IReadOnlyList<Type> targetTypes,
			bool overrideExistingTypeReaders,
			ITypeReader instance)
		{
			Instance = instance;
			OverrideExistingTypeReaders = overrideExistingTypeReaders;
			TargetTypes = targetTypes;
		}

		public TypeReaderInfo(TypeReaderTargetTypesAttribute attribute, ITypeReader instance)
			: this(attribute.TargetTypes, attribute.OverrideExistingTypeReaders, instance)
		{
		}
	}
}