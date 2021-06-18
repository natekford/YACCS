using System.Collections.Generic;

using YACCS.Commands;

namespace YACCS.Interactivity
{
	public interface IInteractivityOptions<in TContext, in TInput>
		: IInteractivityOptions
		where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>> Criteria { get; }
	}
}