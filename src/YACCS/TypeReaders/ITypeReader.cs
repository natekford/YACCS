using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader
	{
		Type ContextType { get; }
		Type OutputType { get; }

		ITask<ITypeReaderResult> ReadAsync(IContext context, ReadOnlyMemory<string> input);
	}
}