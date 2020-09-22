using System;

using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public interface ITypeReaderCollection
	{
		ITypeReader GetReader(Type type);
	}
}