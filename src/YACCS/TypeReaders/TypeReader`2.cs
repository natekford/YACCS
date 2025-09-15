using MorseCode.ITask;

using System;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <inheritdoc />
public abstract class TypeReader<TContext, TValue> : TypeReader<TValue>
	where TContext : IContext
{
	/// <inheritdoc />
	public override Type ContextType { get; } = typeof(TContext);

	/// <inheritdoc cref="ReadAsync(IContext, ReadOnlyMemory{string})" />
	public abstract ITask<ITypeReaderResult<TValue>> ReadAsync(
		TContext context,
		ReadOnlyMemory<string> input);

	/// <inheritdoc />
	public override ITask<ITypeReaderResult<TValue>> ReadAsync(
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