using System;
using System.Collections.Generic;
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
			// Not sure if this is a sureproof way of dealing with IEnumerables correctly
			// The main issue with this is caching can't be used for each individual value
			// because I don't want to make the interfaces dependent upon PreconditionCache
			if (value is IEnumerable<TValue> tValues)
			{
				return CheckAsync(parameter, tContext, tValues, checkAsync);
			}
			// If the value isn't the correct type that means it's either null or wrong type
			// Null values let checkAsync deal with them, wrong type returns an error
			if (!(value is TValue tValue))
			{
				if (value != null)
				{
					return InvalidParameterResult.InstanceTask;
				}
				tValue = default;
			}
			return checkAsync(parameter, tContext, tValue!);
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
	}
}