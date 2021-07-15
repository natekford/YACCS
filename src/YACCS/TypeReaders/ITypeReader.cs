using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader
	{
		Type OutputType { get; }

		ValueTask<ITypeReaderResult> ReadAsync(IContext context, ReadOnlyMemory<string> input);
	}
}