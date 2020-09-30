using System;
using System.Collections.Generic;
using System.Threading;

namespace YACCS.Commands.Interactivity.Pagination
{
	public class PageOptions<TContext, TInput> : IPageOptions<TContext, TInput>
		where TContext : IContext
	{
		public IEnumerable<ICriterion<TContext, TInput>> Criteria { get; set; }
			= Array.Empty<ICriterion<TContext, TInput>>();
		public int? MaxPage { get; set; }
		public int? StartingPage { get; set; }
		public TimeSpan? Timeout { get; set; }
		public CancellationToken? Token { get; set; }
	}
}