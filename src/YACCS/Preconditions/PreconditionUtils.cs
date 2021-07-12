using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public static class PreconditionUtils
	{
		public static ValueTask<IResult> CheckAsync<TContext, TValue>(
			this IParameterPrecondition _,
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value,
			Func<IImmutableCommand, IImmutableParameter, TContext, TValue, ValueTask<IResult>> checkAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				return new(InvalidContextResult.Instance);
			}
			if (value is TValue tValue)
			{
				return checkAsync(command, parameter, tContext, tValue);
			}
			// If the value passed in is null, let checkAsync deal with it
			if (value is null)
			{
				return checkAsync(command, parameter, tContext, default!);
			}
			// Not sure if this is the best way of dealing with IEnumerables
			//
			// The main issue with this is caching can't be used for each individual value
			// because I don't want to make the interfaces dependent upon PreconditionCache
			if (value is IEnumerable<TValue> tValues)
			{
				return CheckAsync(command, parameter, tContext, tValues, checkAsync);
			}
			// Use the non generic interface to handle non nullable arrays
			// passed to nullable preconditions
			//
			// We can't rely solely on the non generic interface though, because something like
			// 'null is int?' returns false
			if (value is IEnumerable tUntypedValues)
			{
				return CheckAsync(command, parameter, tContext, tUntypedValues, checkAsync);
			}
			return new(InvalidParameterResult.Instance);
		}

		public static ValueTask HandleAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, ValueTask> afterExecutionAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				// We don't need to throw an exception here because CheckAsync should
				// return a result indicating an invalid context type and this should not
				// be called before CheckAsync
				return new();
			}
			return afterExecutionAsync(command, tContext);
		}

		public static ValueTask<IResult> HandleAsync<TContext>(
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

		private static async ValueTask<IResult> CheckAsync<TContext, TValue>(
			IImmutableCommand command,
			IImmutableParameter parameter,
			TContext context,
			IEnumerable<TValue> values,
			Func<IImmutableCommand, IImmutableParameter, TContext, TValue, ValueTask<IResult>> checkAsync)
		{
			foreach (var value in values)
			{
				var result = await checkAsync(command, parameter, context, value).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		private static async ValueTask<IResult> CheckAsync<TContext, TValue>(
			IImmutableCommand command,
			IImmutableParameter parameter,
			TContext context,
			IEnumerable values,
			Func<IImmutableCommand, IImmutableParameter, TContext, TValue, ValueTask<IResult>> checkAsync)
		{
			foreach (var value in values)
			{
				var tValue = value is TValue temp ? temp : default;
				if (value is not null && tValue is null)
				{
					return InvalidParameterResult.Instance;
				}

				var result = await checkAsync(command, parameter, context, tValue!).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}
	}
}