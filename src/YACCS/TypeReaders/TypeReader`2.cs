using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TValue"></typeparam>
public abstract class TypeReader<TContext, TValue> : Attribute, ITypeReader<TContext, TValue>
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

	/// <inheritdoc cref="TypeReaderResult{T}.FromError(IResult)"/>
	protected static ITypeReaderResult<TValue> Error(IResult result)
		=> TypeReaderResult<TValue>.FromError(result);

	/// <inheritdoc cref="TypeReaderResult{T}.FromSuccess(T, int?)"/>
	protected static ITypeReaderResult<TValue> Success(TValue value, int? successfullyParsedCount = null)
		=> TypeReaderResult<TValue>.FromSuccess(value, successfullyParsedCount);

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