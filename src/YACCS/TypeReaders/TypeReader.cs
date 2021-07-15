using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<T> : ITypeReader<T>
	{
		public Type OutputType => typeof(T);

		public abstract ValueTask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input);

		async ValueTask<ITypeReaderResult> ITypeReader.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> await ReadAsync(context, input).ConfigureAwait(false);
	}
}