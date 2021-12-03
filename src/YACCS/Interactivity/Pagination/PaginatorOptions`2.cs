
using YACCS.Commands;

namespace YACCS.Interactivity.Pagination;

/// <inheritdoc cref="IPaginatorOptions{TContext, TInput}"/>
public class PaginatorOptions<TContext, TInput> : IPaginatorOptions<TContext, TInput>
	where TContext : IContext
{
	/// <inheritdoc />
	public IEnumerable<ICriterion<TContext, TInput>> Criteria { get; set; }
		= Array.Empty<ICriterion<TContext, TInput>>();
	/// <inheritdoc />
	public Func<int, Task> DisplayCallback { get; set; }
	/// <inheritdoc />
	public int MaxPage { get; set; }
	/// <inheritdoc />
	public int? StartingPage { get; set; }
	/// <inheritdoc />
	public TimeSpan? Timeout { get; set; }
	/// <inheritdoc />
	public CancellationToken? Token { get; set; }

	/// <summary>
	/// Creates a new <see cref="PaginatorOptions{TContext, TInput}"/>.
	/// </summary>
	/// <param name="maxPage">
	/// <inheritdoc cref="MaxPage" path="/summary"/>
	/// </param>
	/// <param name="displayCallback">
	/// <inheritdoc cref="DisplayCallback" path="/summary"/>
	/// </param>
	public PaginatorOptions(int maxPage, Func<int, Task> displayCallback)
	{
		MaxPage = maxPage;
		DisplayCallback = displayCallback;
	}
}
