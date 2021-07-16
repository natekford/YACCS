using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public interface ITypeReader<in TContext, out TValue> : ITypeReader<TValue>
		where TContext : IContext
	{
		ITask<ITypeReaderResult<TValue>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input);
	}
}