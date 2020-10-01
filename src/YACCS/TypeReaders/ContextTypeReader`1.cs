using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<T> : TypeReader<T> where T : IContext
	{
		public override ITask<ITypeReaderResult<T>> ReadAsync(IContext context, string input)
		{
			if (!(context is T tContext))
			{
				return TypeReaderResult<T>.Failure.ITask;
			}
			return TypeReaderResult<T>.FromSuccess(tContext).AsITask();
		}
	}
}