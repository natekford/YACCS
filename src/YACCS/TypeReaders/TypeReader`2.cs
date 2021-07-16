using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<TContext, TValue> : ITypeReader<TContext, TValue>
		where TContext : IContext
	{
		public Type OutputType => typeof(TValue);
		protected static ITask<ITypeReaderResult<TValue>> InvalidContext { get; }
			= TypeReaderResult<TValue>.FromError(InvalidContextResult.Instance).AsITask();

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

		protected virtual ITask<ITypeReaderResult<TValue>> ReadInternalAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is not TContext tContext)
			{
				return InvalidContext;
			}
			return ReadAsync(tContext, input);
		}
	}
}