using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader<out TValue> : ITypeReader
	{
		new ITask<ITypeReaderResult<TValue>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input);
	}
}