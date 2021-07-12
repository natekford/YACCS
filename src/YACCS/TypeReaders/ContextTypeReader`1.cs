using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<T> : TypeReader<T> where T : IContext
	{
		public override ITask<ITypeReaderResult<T>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is not T tContext)
			{
				return TypeReaderResult<T>.FailureInstance;
			}
			return TypeReaderResult<T>.FromSuccess(tContext).AsITask();
		}
	}
}