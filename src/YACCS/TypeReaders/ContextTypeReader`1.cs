using MorseCode.ITask;

using System;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Determines if the passed in context implements <typeparamref name="TContext"/>.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class ContextTypeReader<TContext> : TypeReader<TContext>
	where TContext : IContext
{
	/// <inheritdoc />
	public override Type ContextType { get; } = typeof(TContext);

	/// <inheritdoc />
	/// <remarks>This will never end up actually getting called.</remarks>
	public override ITask<ITypeReaderResult<TContext>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		if (context is not TContext tContext)
		{
			return CachedResults<TContext>.InvalidContext.Task;
		}
		return Success(tContext).AsITask();
	}
}