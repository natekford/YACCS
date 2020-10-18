using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public static class PreconditionUtils
	{
		public static Task HandleAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, Task> afterExecutionAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				// We don't need to throw an exception here because CheckAsync should
				// return a result indicating an invalid context type and this should not
				// be called before CheckAsync
				return InvalidContextResult.Instance.Task;
			}
			return afterExecutionAsync(command, tContext);
		}

		public static Task<IResult> HandleAsync<TContext>(
			this IPrecondition _,
			IImmutableCommand command,
			IContext context,
			Func<IImmutableCommand, TContext, Task<IResult>> checkAsync)
			where TContext : IContext
		{
			if (context is not TContext tContext)
			{
				return InvalidContextResult.Instance.Task;
			}
			return checkAsync(command, tContext);
		}
	}
}