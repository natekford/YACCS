using System;

namespace YACCS.TypeReaders
{
	public interface ITypeReaderGeneratorAttribute
	{
		ITypeReader GenerateTypeReader(Type type);
	}
}