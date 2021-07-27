using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.TypeReaders
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TypeReaderTargetTypesAttribute : Attribute
	{
		public bool OverrideExistingTypeReaders { get; }
		public IReadOnlyList<Type> TargetTypes { get; }

		public TypeReaderTargetTypesAttribute(params Type[] types)
		{
			TargetTypes = types.ToImmutableArray();
		}
	}
}