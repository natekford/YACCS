using YACCS.Commands;

namespace YACCS.Interactivity.Pagination
{
	public interface IPageOptions<in TContext, in TInput>
		: IInteractivityOptions<TContext, TInput>
		where TContext : IContext
	{
		int MaxPage { get; }
		int? StartingPage { get; }
	}
}