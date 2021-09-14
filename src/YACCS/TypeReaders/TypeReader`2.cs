
using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Parses a <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TValue"></typeparam>
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

		/// <summary>
		/// Creates a failure result with <paramref name="result"/>.
		/// </summary>
		/// <param name="result">The result to wrap.</param>
		/// <returns>A failure result.</returns>
		protected static ITypeReaderResult<TValue> Error(IResult result)
			=> TypeReaderResult<TValue>.FromError(result);

		/// <summary>
		/// Creates a success result with <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value which was parsed.</param>
		/// <returns>A success result.</returns>
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