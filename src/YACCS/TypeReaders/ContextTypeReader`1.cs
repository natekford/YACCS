using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<T> : ITypeReader<T> where T : IContext
	{
		public Task<ITypeReaderResult<T>> ReadAsync(IContext context, string input)
		{
			if (!(context is T castedContext))
			{
				return TypeReaderResult<T>.FailureTask;
			}
			return TypeReaderResult<T>.FromSuccess(castedContext).AsTask();
		}

		async Task<ITypeReaderResult> ITypeReader.ReadAsync(IContext context, string input)
			=> await ReadAsync(context, input).ConfigureAwait(false);
	}
}