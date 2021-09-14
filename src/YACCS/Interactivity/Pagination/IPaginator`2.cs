
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Interactivity.Pagination
{
	/// <summary>
	/// Defines a method for paginating from a source of input.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TInput"></typeparam>
	public interface IPaginator<TContext, TInput> where TContext : IContext
	{
		/// <summary>
		/// Paginates through something via any <typeparamref name="TInput"/> received.
		/// </summary>
		/// <param name="context">The context which initialized pagination.</param>
		/// <param name="options">The options to use while paginating.</param>
		/// <returns>A result indicating success or failure.</returns>
		Task<IResult> PaginateAsync(TContext context, IPaginatorOptions<TContext, TInput> options);
	}
}