using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class Precondition<TContext> : IPrecondition<TContext> where TContext : IContext
	{
		public abstract Task<IResult> CheckAsync(CommandInfo info, TContext context);

		public Task<IResult> CheckAsync(CommandInfo info, IContext context)
		{
			if (!(context is TContext castedContext))
			{
				return InvalidContextResult.InstanceTask;
			}
			return CheckAsync(info, castedContext);
		}
	}
}