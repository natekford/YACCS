using System;

using YACCS.TypeReaders;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false)]
	public class OverrideTypeReaderAttribute : Attribute, IOverrideTypeReaderAttribute
	{
		public ITypeReader Reader { get; }

		public OverrideTypeReaderAttribute(Type type)
		{
			Reader = type.CreateInstance<ITypeReader>();
		}
	}
}