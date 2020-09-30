using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity.Pagination
{
	public interface IPageDisplayer<TContext> where TContext : IContext
	{
		Task DisplayAsync(TContext context, int page);
	}
}