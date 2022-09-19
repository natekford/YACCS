using YACCS.Commands;
using YACCS.Interactivity.Input;
using YACCS.Interactivity.Pagination;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity;

/// <summary>
/// Utilities for interactivity.
/// </summary>
public static class InteractivityUtils
{
	/// <summary>
	/// Extension method for type inference.
	/// <paramref name="exampleValueForTypeInference"/> is not used.
	/// </summary>
	/// <inheritdoc cref="IInput{TContext, TInput}.GetAsync{TValue}(TContext, IInputOptions{TContext, TInput, TValue})"/>
	public static Task<ITypeReaderResult<TValue>> InferGetAsync<TContext, TInput, TValue>(
		this IInput<TContext, TInput> input,
		TContext context,
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
		TValue? exampleValueForTypeInference,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
		InputOptions<TContext, TInput, TValue> options)
		where TContext : IContext
		=> input.GetAsync(context, options);

	/// <summary>
	/// Extension method for type inference.
	/// </summary>
	/// <inheritdoc cref="IPaginator{TContext, TInput}.PaginateAsync(TContext, IPaginatorOptions{TContext, TInput})"/>
	public static Task<IResult> InferPaginateAsync<TContext, TInput>(
		this IPaginator<TContext, TInput> paginator,
		TContext context,
		PaginatorOptions<TContext, TInput> options)
		where TContext : IContext
		=> paginator.PaginateAsync(context, options);

	internal static IEnumerable<T> ThisOrEmpty<T>(this IEnumerable<T>? enumerable)
		=> enumerable ?? Array.Empty<T>();
}