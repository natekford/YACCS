using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public static class ParameterPreconditionUtils
	{
		public static Task<IResult> CheckAsync<TContext, TValue>(
			this IParameterPrecondition _,
			ParameterInfo parameter,
			IContext context,
			object? value,
			Func<ParameterInfo, TContext, TValue, Task<IResult>> checkAsync)
			where TContext : IContext
		{
			if (!(context is TContext tContext))
			{
				return InvalidContextResult.InstanceTask;
			}
			if (value is TValue tValue)
			{
				return checkAsync(parameter, tContext, tValue);
			}
			// If the value passed in is null, let checkAsync deal with it
			if (value is null)
			{
				return checkAsync(parameter, tContext, default!);
			}
			// Not sure if this is the best way of dealing with IEnumerables
			//
			// The main issue with this is caching can't be used for each individual value
			// because I don't want to make the interfaces dependent upon PreconditionCache
			if (value is IEnumerable<TValue> tValues)
			{
				return CheckAsync(parameter, tContext, tValues, checkAsync);
			}
			// Use the non generic interface to handle non nullable arrays
			// passed to nullable preconditions
			//
			// We can't rely solely on the non generic interface though, because something like
			// 'null is int?' returns false
			if (value is IEnumerable tUntypedValues)
			{
				return CheckAsync(parameter, tContext, tUntypedValues, checkAsync);
			}
			return InvalidParameterResult.InstanceTask;
		}

		private static async Task<IResult> CheckAsync<TContext, TValue>(
			ParameterInfo parameter,
			TContext context,
			IEnumerable<TValue> values,
			Func<ParameterInfo, TContext, TValue, Task<IResult>> checkAsync)
		{
			foreach (var value in values)
			{
				var result = await checkAsync(parameter, context, value).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}

		private static async Task<IResult> CheckAsync<TContext, TValue>(
			ParameterInfo parameter,
			TContext context,
			IEnumerable values,
			Func<ParameterInfo, TContext, TValue, Task<IResult>> checkAsync)
		{
			foreach (var value in values)
			{
				if (!(value is TValue tValue))
				{
					return InvalidParameterResult.Instance;
				}

				var result = await checkAsync(parameter, context, tValue).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return SuccessResult.Instance;
		}
	}
}