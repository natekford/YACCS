using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public static class PreconditionUtils
	{
		public static Task AfterExecutionAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, Task> afterExecutionAsync)
		{
			if (!(context is TContext tContext))
			{
				return InvalidContextResult.Instance.Task;
			}
			return afterExecutionAsync(command, tContext);
		}

		public static Task<IResult> CheckAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, Task<IResult>> checkAsync)
			where TContext : IContext
		{
			if (!(context is TContext tContext))
			{
				return InvalidContextResult.Instance.Task;
			}
			return checkAsync(command, tContext);
		}
	}
}