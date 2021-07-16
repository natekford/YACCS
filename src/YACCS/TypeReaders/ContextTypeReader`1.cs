using System;

using MorseCode.ITask;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public class ContextTypeReader<TContext> : TypeReader<TContext, TContext>
		where TContext : IContext
	{
		public override ITask<ITypeReaderResult<TContext>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input)
		{
			// This won't be hit because TypeReader`2 already checks for context type
			// In case someone ever calls this directly this should remain
			if (context is not TContext tContext)
			{
				return InvalidContext;
			}
			return TypeReaderResult<TContext>.FromSuccess(tContext).AsITask();
		}
	}
}