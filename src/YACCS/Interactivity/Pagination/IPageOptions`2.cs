using YACCS.Commands;

namespace YACCS.Interactivity.Pagination
{
	public interface IPageOptions<TContext, TInput>
		: IInteractivityOptions<TContext, TInput>
		where TContext : IContext
	{
		int? MaxPage { get; }
		int? StartingPage { get; }
	}
}