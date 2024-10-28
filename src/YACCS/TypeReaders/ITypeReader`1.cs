using MorseCode.ITask;

using System;

using YACCS.Commands;

namespace YACCS.TypeReaders;

/// <inheritdoc />
public interface ITypeReader<out TValue> : ITypeReader
{
	/// <inheritdoc cref="ITypeReader.ReadAsync(IContext, ReadOnlyMemory{string})" />
	new ITask<ITypeReaderResult<TValue>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input);
}