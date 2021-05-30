using System;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity.Pagination
{
	public abstract class Paginator<TContext, TInput>
		: Interactivity<TContext, TInput>, IPaginator<TContext, TInput>
		where TContext : IContext
	{
		public virtual async Task PaginateAsync(
			TContext context,
			IPageDisplayer<TContext, TInput> displayer,
			IPageOptions<TContext, TInput> options)
		{
			var page = options.StartingPage ?? 0;
			var maxPage = options.MaxPage ?? 0;

			while (true)
			{
				await displayer.DisplayAsync(context, page).ConfigureAwait(false);

				var result = await HandleInteraction<int?>(context, options, e => new OnInput(async i =>
				{
					foreach (var criterion in options.Criteria)
					{
						var result = await criterion.JudgeAsync(context, i).ConfigureAwait(false);
						if (!result.IsSuccess)
						{
							return result;
						}
					}

					e.SetResult(displayer.Convert(i));
					return SuccessResult.Instance.Sync;
				})).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess || !result.Value.HasValue)
				{
					return;
				}

				page = Mod(page + result.Value.Value, maxPage);
			}
		}

		protected static int Mod(double a, double b)
		{
			if (b == 0)
			{
				return 0;
			}
			return (int)(a - (b * Math.Floor(a / b)));
		}
	}
}