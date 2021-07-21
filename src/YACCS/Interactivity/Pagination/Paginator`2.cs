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

				var result = await HandleInteraction<int?>(context, options, async (task, input) =>
				{
					foreach (var criterion in options.Criteria)
					{
						var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
						if (!result.IsSuccess)
						{
							return result;
						}
					}

					task.SetResult(displayer.Convert(input));
					return SuccessResult.Instance;
				}).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess || !result.Value.HasValue)
				{
					return;
				}

				page = Mod(page + result.Value.Value, maxPage);
			}
		}

		protected static int Mod(int a, int b)
			=> b == 0 ? 0 : (int)(a - (b * Math.Floor((double)a / b)));
	}
}