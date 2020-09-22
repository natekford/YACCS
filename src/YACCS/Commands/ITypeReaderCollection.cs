using System;

using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public interface ITypeReaderCollection
	{
		bool TryGetReader(Type type, out ITypeReader result);
	}
}