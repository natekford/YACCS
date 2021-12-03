
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Interactivity.Pagination;

/// <summary>
/// The base class for handling pagination.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
public abstract class Paginator<TContext, TInput>
	: Interactivity<TContext, TInput>, IPaginator<TContext, TInput>
	where TContext : IContext
{
	/// <inheritdoc />
	public virtual async Task<IResult> PaginateAsync(
		TContext context,
		IPaginatorOptions<TContext, TInput> options)
	{
		var page = options.StartingPage ?? 0;

		// Display the starting page
		await options.DisplayCallback.Invoke(page).ConfigureAwait(false);

		var eventTrigger = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
		var result = await HandleInteractionAsync(context, options, eventTrigger, async input =>
		{
			foreach (var criterion in options.Criteria)
			{
				var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
				if (!result.IsSuccess)
				{
					return result;
				}
			}

			var converted = await ConvertAsync(input).ConfigureAwait(false);
			if (!converted.HasValue)
			{
					// Converted value is null, indicating we should stop
					// Easiest way to do that is to just set the dummy event trigger
					eventTrigger.SetResult(null);
				return InteractionEndedResult.Instance;
			}

			page = GetNewPage(page, options.MaxPage, converted.Value);
				// Display the new page
				await options.DisplayCallback.Invoke(page).ConfigureAwait(false);
			return SuccessResult.Instance;
		}).ConfigureAwait(false);

		// Since the only way to get SuccessResult is it eventTrigger gets set,
		// return InteractionEndedResult on SuccessResult
		if (result.InnerResult is SuccessResult)
		{
			return InteractionEndedResult.Instance;
		}
		return result.InnerResult;
	}

	/// <summary>
	/// Converts <paramref name="input"/> into an <see cref="int"/>
	/// so it can be used in pagination.
	/// </summary>
	/// <param name="input">The input to convert.</param>
	/// <returns>An <see cref="int"/> representing <paramref name="input"/>.</returns>
	protected abstract Task<int?> ConvertAsync(TInput input);

	/// <summary>
	/// Gets the new page to navigate to.
	/// </summary>
	/// <param name="current">The current page.</param>
	/// <param name="max">The maximum allowed page.</param>
	/// <param name="diff">The distance to move from the current page.</param>
	/// <returns>An <see cref="int"/> representing the new page.</returns>
	/// <remarks>
	/// By default this supports wrapping around.
	/// <br/>
	/// (current: 5, max: 7: diff: 5) => current = 3
	/// <br/>
	/// (current: 5, max: 7: diff: -6) => current = 6
	/// </remarks>
	protected virtual int GetNewPage(int current, int max, int diff)
		=> Mod(current + diff, max);

	private static int Mod(int a, int b)
		=> b == 0 ? 0 : (int)(a - (b * Math.Floor((double)a / b)));
}
