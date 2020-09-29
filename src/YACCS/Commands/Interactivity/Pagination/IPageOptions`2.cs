using System;
using System.Collections.Generic;
using System.Threading;

namespace YACCS.Commands.Interactivity.Pagination
{
	public interface IPageOptions<TContext, TInput> where TContext : IContext
	{
		IEnumerable<ICriterion<TContext, TInput>>? Criteria { get; }
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
		int? StartingPage { get; }
		int? MaxPage { get; }
	}
}