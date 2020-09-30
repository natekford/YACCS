using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity.Pagination
{
	public interface IPageDisplayer<TContext, TInput> where TContext : IContext
	{
		int? Convert(TInput input);

		Task DisplayAsync(TContext context, int page);
	}
}