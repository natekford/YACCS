using YACCS.Commands;

namespace YACCS.Interactivity.Pagination;

/// <inheritdoc cref="IPaginatorOptions{TContext, TInput}"/>
public record PaginatorOptions<TContext, TInput>(
	Func<int, Task> DisplayCallback,
	IEnumerable<ICriterion<TContext, TInput>>? Criteria = null,
	int? MaxPage = null,
	int? StartingPage = null,
	TimeSpan? Timeout = null,
	CancellationToken? Token = null
) : IPaginatorOptions<TContext, TInput> where TContext : IContext;