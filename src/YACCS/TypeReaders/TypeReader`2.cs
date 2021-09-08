using System;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	public abstract class TypeReader<TContext, TValue> : ITypeReader<TContext, TValue>
		where TContext : IContext
	{
		/// <inheritdoc />
		public Type ContextType => typeof(TContext);
		/// <inheritdoc />
		public Type OutputType => typeof(TValue);

		/// <inheritdoc />
		public abstract ITask<ITypeReaderResult<TValue>> ReadAsync(
			TContext context,
			ReadOnlyMemory<string> input);

		ITask<ITypeReaderResult> ITypeReader.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> PrivateReadAsync(context, input);

		ITask<ITypeReaderResult<TValue>> ITypeReader<TValue>.ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> PrivateReadAsync(context, input);

		protected static ITypeReaderResult<TValue> Error(IResult result)
			=> TypeReaderResult<TValue>.FromError(result);

		protected static ITypeReaderResult<TValue> Success(TValue value)
			=> TypeReaderResult<TValue>.FromSuccess(value);

		private ITask<ITypeReaderResult<TValue>> PrivateReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (context is not TContext tContext)
			{
				return CachedResults<TValue>.InvalidContext.Task;
			}
			return ReadAsync(tContext, input);
		}
	}
}