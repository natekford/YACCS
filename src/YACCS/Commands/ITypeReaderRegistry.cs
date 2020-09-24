using System;
using System.Diagnostics.CodeAnalysis;

using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public interface ITypeReaderRegistry
	{
		void Register(ITypeReader reader, Type type);

		bool TryGetReader(Type type, [NotNullWhen(true)] out ITypeReader? reader);
	}
}