using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<T> : TypeReader<T> where T : IContext
	{
		public override Task<ITypeReaderResult<T>> ReadAsync(IContext context, string input)
		{
			if (!(context is T tContext))
			{
				return TypeReaderResult<T>.FailureTask;
			}
			return TypeReaderResult<T>.FromSuccess(tContext).AsTask();
		}
	}
}