using System;
using System.Collections.Generic;
using System.Threading;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractivityOptions<TContext, TInput> where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>> Criteria { get; }
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
	}
}