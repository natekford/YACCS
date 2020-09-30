using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<T> : ITypeReader<T>
	{
		public Type OutputType { get; } = typeof(T);

		public abstract Task<ITypeReaderResult<T>> ReadAsync(IContext context, string input);

		async Task<ITypeReaderResult> ITypeReader.ReadAsync(IContext context, string input)
			=> await ReadAsync(context, input).ConfigureAwait(false);
	}
}