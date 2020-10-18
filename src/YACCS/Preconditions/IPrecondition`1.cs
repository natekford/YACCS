using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition<in TContext> : IPrecondition where TContext : IContext
	{
		Task AfterExecutionAsync(IImmutableCommand command, TContext context, Exception? exception);

		Task BeforeExecutionAsync(IImmutableCommand command, TContext context);

		Task<IResult> CheckAsync(IImmutableCommand command, TContext context);
	}
}