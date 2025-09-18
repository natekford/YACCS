using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions;

/// <summary>
/// The base class for a precondition.
/// </summary>
/// <typeparam name="TContext"></typeparam>
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
public abstract class Precondition<TContext>
	: GroupablePrecondition, IPrecondition<TContext>
	where TContext : IContext
{
	/// <inheritdoc />
	public virtual Task AfterExecutionAsync(
		IImmutableCommand command,
		TContext context,
		Exception? exception)
		=> Task.CompletedTask;

	/// <inheritdoc />
	public virtual Task BeforeExecutionAsync(
		IImmutableCommand command,
		TContext context)
		=> Task.CompletedTask;

	/// <inheritdoc />
	public abstract ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		TContext context);

	Task IPrecondition.AfterExecutionAsync(
		IImmutableCommand command,
		IContext context,
		Exception? exception)
		=> AfterExecutionAsync(command, context, exception);

	Task IPrecondition.BeforeExecutionAsync(
		IImmutableCommand command,
		IContext context)
		=> BeforeExecutionAsync(command, context);

	ValueTask<IResult> IPrecondition.CheckAsync(
		IImmutableCommand command,
		IContext context)
		=> CheckAsyncInternal(command, context);

	/// <summary>
	/// Invokes <see cref="AfterExecutionAsync(IImmutableCommand, TContext, Exception?)"/>
	/// after handling type checking.
	/// </summary>
	/// <inheritdoc cref="AfterExecutionAsync(IImmutableCommand, TContext, Exception?)"/>
	protected virtual Task AfterExecutionAsync(
		IImmutableCommand command,
		IContext context,
		Exception? exception)
	{
		if (context is not TContext tContext)
		{
			return Task.FromResult(Result.InvalidContext);
		}
		return AfterExecutionAsync(command, tContext, exception);
	}

	/// <summary>
	/// Invokes <see cref="BeforeExecutionAsync(IImmutableCommand, TContext)"/>
	/// after handling type checking.
	/// </summary>
	/// <inheritdoc cref="BeforeExecutionAsync(IImmutableCommand, TContext)"/>
	protected virtual Task BeforeExecutionAsync(
		IImmutableCommand command,
		IContext context)
	{
		if (context is not TContext tContext)
		{
			return Task.FromResult(Result.InvalidContext);
		}
		return BeforeExecutionAsync(command, tContext);
	}

	/// <summary>
	/// Invokes <see cref="CheckAsync(IImmutableCommand, TContext)"/>
	/// after handling type checking.
	/// </summary>
	/// <inheritdoc cref="CheckAsync(IImmutableCommand, TContext)"/>
	protected virtual ValueTask<IResult> CheckAsyncInternal(
		IImmutableCommand command,
		IContext context)
	{
		if (context is not TContext tContext)
		{
			return new(Result.InvalidContext);
		}
		return CheckAsync(command, tContext);
	}
}