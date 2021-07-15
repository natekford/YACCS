using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader<T> : ITypeReader
	{
		new ValueTask<ITypeReaderResult<T>> ReadAsync(IContext context, ReadOnlyMemory<string> input);
	}
}