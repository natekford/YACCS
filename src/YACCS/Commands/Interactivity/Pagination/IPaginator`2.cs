using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity.Pagination
{
	public interface IPaginator<TContext, TInput> where TContext : IContext
	{
		Task PaginateAsync(
			TContext context,
			IPageDisplayer<TContext> displayer,
			IPageOptions<TContext, TInput> options);
	}
}