using System;
using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Interactivity.Pagination
{
	public interface IPaginatorOptions<in TContext, in TInput>
		: IInteractivityOptions<TContext, TInput>
		where TContext : IContext
	{
		Func<int, Task> DisplayCallback { get; }
		int MaxPage { get; }
		int? StartingPage { get; }
	}
}