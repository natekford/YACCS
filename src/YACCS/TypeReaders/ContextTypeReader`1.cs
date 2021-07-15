using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<T> : TypeReader<T> where T : IContext
	{
		public override ValueTask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is not T tContext)
			{
				return new(TypeReaderResult<T>.FailureInstance);
			}
			return new(TypeReaderResult<T>.FromSuccess(tContext));
		}
	}
}