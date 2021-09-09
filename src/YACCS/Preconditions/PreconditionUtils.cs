using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <inheritdoc cref="IParameterPrecondition.CheckAsync(CommandMeta, IContext, object?)"/>
	public delegate ValueTask<IResult> CheckAsync<TContext, TValue>(
		CommandMeta meta,
		TContext context,
		TValue value);

	/// <summary>
	/// Utilities for preconditions.
	/// </summary>
	public static class PreconditionUtils
	{
		private static Task InvalidContext { get; }
			= Task.FromResult(InvalidContextResult.Instance);

		/// <summary>
		/// Invokes <paramref name="checkAsync"/> after pattern matching arguments.
		/// </summary>
		/// <inheritdoc cref="IParameterPrecondition.CheckAsync(CommandMeta, IContext, object?)"/>
		public static ValueTask<IResult> CheckAsync<TContext, TValue>(
			this IParameterPrecondition _,
			CommandMeta meta,
			IContext context,
			object? value,
			CheckAsync<TContext, TValue> checkAsync)
		{
			if (context is not TContext tContext)
			{
				return new(InvalidContextResult.Instance);
			}
			if (value is TValue tValue)
			{
				return checkAsync(meta, tContext, tValue);
			}
			// If the value passed in is null, let checkAsync deal with it
			if (value is null)
			{
				return checkAsync(meta, tContext, default!);
			}
			// Not sure if this is the best way of dealing with IEnumerables
			//
			// The main issue with this is caching can't be used for each individual value
			// because I don't want to make the interfaces dependent upon PreconditionCache
			if (value is IEnumerable<TValue> tValues)
			{
				return CheckAsync(meta, tContext, tValues, checkAsync);
			}
			// Use the non generic interface to handle non nullable arrays
			// passed to nullable preconditions
			//
			// We can't rely solely on the non generic interface though, because something like
			// 'null is int?' returns false
			if (value is IEnumerable tUntypedValues)
			{
				return CheckAsync(meta, tContext, tUntypedValues, checkAsync);
			}
			return new(InvalidParameterResult.Instance);
		}

		/// <summary>
		/// Invokes <paramref name="checkAsync"/> after pattern matching arguments.
		/// </summary>
		/// <inheritdoc cref="IPrecondition.CheckAsync(IImmutableCommand, IContext)"/>
		public static ValueTask<IResult> CheckAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, ValueTask<IResult>> checkAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				return new(InvalidContextResult.Instance);
			}
			return checkAsync(command, tContext);
		}

		/// <summary>
		/// Invokes <paramref name="executionAsync"/> after pattern matching arguments.
		/// </summary>
		/// <inheritdoc cref="IPrecondition.AfterExecutionAsync(IImmutableCommand, IContext, Exception?)"/>
		public static Task HandleAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, Task> executionAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				// We don't need to throw an exception here because CheckAsync should
				// return a result indicating an invalid context type and this should not
				// be called before CheckAsync
				return InvalidContext;
			}
			return executionAsync(command, tContext);
		}

		private static async ValueTask<IResult> CheckAsync<TContext, TValue>(
			CommandMeta meta,
			TContext context,
			IEnumerable<TValue> values,
			CheckAsync<TContext, TValue> checkAsync)
		{
			foreach (var value in values)
			{
				var result = await checkAsync(meta, context, value).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		private static async ValueTask<IResult> CheckAsync<TContext, TValue>(
			CommandMeta meta,
			TContext context,
			IEnumerable values,
			CheckAsync<TContext, TValue> checkAsync)
		{
			foreach (var value in values)
			{
				var tValue = value is TValue temp ? temp : default;
				if (value is not null && tValue is null)
				{
					return InvalidParameterResult.Instance;
				}

				var result = await checkAsync(meta, context, tValue!).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}
	}
}