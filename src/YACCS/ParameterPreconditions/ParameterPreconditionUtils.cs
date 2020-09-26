using System;
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
	}
}