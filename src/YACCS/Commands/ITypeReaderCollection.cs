using System;
using System.Diagnostics.CodeAnalysis;

using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public interface ITypeReaderCollection
	{
		bool TryGetReader(Type type, [NotNullWhen(true)] out ITypeReader? reader);
	}
}