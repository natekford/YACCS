using YACCS.Commands;

namespace YACCS.Interactivity.Pagination;

/// <summary>
/// Interactivity options which support pagination.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
public interface IPaginatorOptions<in TContext, in TInput> :
	IInteractivityOptions<TContext, TInput>
	where TContext : IContext
{
	/// <summary>
	/// The callback to invoke when a new page is navigated to.
	/// </summary>
	Func<int, Task> DisplayCallback { get; }
	/// <summary>
	/// The max page allowed.
	/// </summary>
	int MaxPage { get; }
	/// <summary>
	/// The starting page.
	/// </summary>
	int? StartingPage { get; }
}