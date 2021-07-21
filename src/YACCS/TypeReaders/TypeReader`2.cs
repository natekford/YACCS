using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<TContext, TValue> : ITypeReader<TContext, TValue>
		where TContext : IContext
	{
		public Type ContextType => typeof(TContext);
		public Type OutputType => typeof(TValue);

		public abstract ITask<ITypeReaderResult<TValue>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input);

		ITask<ITypeReaderResult> ITypeReader.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> ReadInternalAsync(context, input);

		ITask<ITypeReaderResult<TValue>> ITypeReader<TValue>.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> ReadInternalAsync(context, input);

		protected ITypeReaderResult<TValue> Error(IResult result)
			=> TypeReaderResult<TValue>.FromError(result);

		protected virtual ITask<ITypeReaderResult<TValue>> ReadInternalAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is not TContext tContext)
			{
				return CachedResults<TValue>.InvalidContextTask;
			}
			return ReadAsync(tContext, input);
		}

		protected ITypeReaderResult<TValue> Success(TValue value)
			=> TypeReaderResult<TValue>.FromSuccess(value);
	}
}