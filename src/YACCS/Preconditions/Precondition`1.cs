using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class Precondition<TContext> : IPrecondition<TContext> where TContext : IContext
	{
		public virtual Task AfterExecutionAsync(IImmutableCommand command, TContext context)
			=> Task.CompletedTask;

		public Task AfterExecutionAsync(IImmutableCommand command, IContext context)
			=> this.AfterExecutionAsync<TContext>(command, context, AfterExecutionAsync);

		public abstract Task<IResult> CheckAsync(IImmutableCommand command, TContext context);

		public Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
			=> this.CheckAsync<TContext>(command, context, CheckAsync);
	}
}