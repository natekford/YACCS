using YACCS.Commands;

namespace YACCS.Interactivity.Pagination;

/// <summary>
/// Interactivity options which support pagination.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <param name="DisplayCallback">The callback to invoke when a new page is navigated to.</param>
/// <param name="MaxPage">The max page allowed.</param>
/// <param name="StartingPage">The starting page.</param>
public record PaginatorOptions<TContext, TInput>(
	Func<int, Task> DisplayCallback,
	int? MaxPage = null,
	int? StartingPage = null
) : InteractivityOptions<TContext, TInput> where TContext : IContext;