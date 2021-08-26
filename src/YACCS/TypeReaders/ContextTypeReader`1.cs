using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<TContext> : TypeReader<TContext, TContext>
		where TContext : IContext
	{
		public override ITask<ITypeReaderResult<TContext>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is null)
			{
				return CachedResults<TContext>.InvalidContext.Task;
			}
			return Success(context).AsITask();
		}
	}
}