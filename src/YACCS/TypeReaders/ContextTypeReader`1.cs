using MorseCode.ITask;

using System;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <summary>
/// Determines if the passed in context implements <typeparamref name="TContext"/>.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class ContextTypeReader<TContext> : TypeReader<TContext, TContext>
	where TContext : IContext
{
	/// <inheritdoc />
	/// <remarks>This will never end up actually getting called.</remarks>
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