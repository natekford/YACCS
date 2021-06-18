using System.Threading.Tasks;

using YACCS.Commands;

namespace YACCS.Interactivity.Pagination
{
	public interface IPageDisplayer<TContext, TInput> where TContext : IContext
	{
		int? Convert(TInput input);

		Task DisplayAsync(TContext context, int page);
	}
}