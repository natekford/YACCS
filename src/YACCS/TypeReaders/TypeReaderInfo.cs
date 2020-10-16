using System;
using System.Collections.Generic;

namespace YACCS.TypeReaders
{
	public readonly struct TypeReaderInfo
	{
		public ITypeReader Instance { get; }
		public IReadOnlyList<Type> TargetTypes { get; }

		public TypeReaderInfo(IReadOnlyList<Type> targetTypes, ITypeReader instance)
		{
			Instance = instance;
			TargetTypes = targetTypes;
		}
	}
}