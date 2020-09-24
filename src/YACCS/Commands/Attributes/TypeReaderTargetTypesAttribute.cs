using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TypeReaderTargetTypesAttribute : Attribute, ITypeReaderTargetTypeAttribute
	{
		public IReadOnlyList<Type> TargetTypes { get; }

		public TypeReaderTargetTypesAttribute(params Type[] types)
		{
			TargetTypes = types.ToImmutableArray();
		}
	}
}