using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Interactivity.Pagination
{
	public class PaginatorOptions<TContext, TInput> : IPaginatorOptions<TContext, TInput>
		where TContext : IContext
	{
		public IEnumerable<ICriterion<TContext, TInput>> Criteria { get; set; }
			= Array.Empty<ICriterion<TContext, TInput>>();
		public Func<int, Task> DisplayCallback { get; set; }
		public int MaxPage { get; set; }
		public int? StartingPage { get; set; }
		public TimeSpan? Timeout { get; set; }
		public CancellationToken? Token { get; set; }

		public PaginatorOptions(int maxPage, Func<int, Task> displayCallback)
		{
			MaxPage = maxPage;
			DisplayCallback = displayCallback;
		}
	}
}