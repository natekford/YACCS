
using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	/// <inheritdoc />
	public interface ITypeReader<in TContext, out TValue> : ITypeReader<TValue>
		where TContext : IContext
	{
		/// <inheritdoc cref="ITypeReader.ReadAsync(IContext, ReadOnlyMemory{string})" />
		ITask<ITypeReaderResult<TValue>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input);
	}
}