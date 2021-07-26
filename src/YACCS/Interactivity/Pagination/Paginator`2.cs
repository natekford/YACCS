using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Interactivity.Pagination
{
	public abstract class Paginator<TContext, TInput>
		: Interactivity<TContext, TInput>, IPaginator<TContext, TInput>
		where TContext : IContext
	{
		public virtual async Task<IResult> PaginateAsync(
			TContext context,
			IPageOptions<TContext, TInput> options)
		{
			var page = options.StartingPage ?? 0;

			// Display the starting page
			await DisplayAsync(context, page).ConfigureAwait(false);

			var eventTrigger = new TaskCompletionSource<object?>();
			var result = await HandleInteraction(context, options, eventTrigger, async input =>
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
				await DisplayAsync(context, page).ConfigureAwait(false);
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

		protected static int Mod(int a, int b)
			=> b == 0 ? 0 : (int)(a - (b * Math.Floor((double)a / b)));

		protected abstract Task<int?> ConvertAsync(TInput input);

		protected abstract Task DisplayAsync(TContext context, int page);

		protected virtual int GetNewPage(int current, int max, int diff)
			=> Mod(current + diff, max);
	}
}