using System.Collections.Generic;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractivityOptions<TContext, TInput> : IInteractivityOptions
		where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>> Criteria { get; }
	}
}