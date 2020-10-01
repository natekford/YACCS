using System;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<T> : ITypeReader<T>
	{
		public Type OutputType => typeof(T);

		public abstract ITask<ITypeReaderResult<T>> ReadAsync(IContext context, string input);

		async Task<ITypeReaderResult> ITypeReader.ReadAsync(IContext context, string input)
			=> await ReadAsync(context, input).ConfigureAwait(false);
	}
}