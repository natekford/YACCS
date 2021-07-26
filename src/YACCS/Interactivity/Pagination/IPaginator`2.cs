using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Interactivity.Pagination
{
	public interface IPaginator<TContext, TInput> where TContext : IContext
	{
		Task<IResult> PaginateAsync(TContext context, IPageOptions<TContext, TInput> options);
	}
}