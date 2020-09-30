using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity.Pagination
{
	public abstract class Paginator<TContext, TInput, TDisplay>
		: InteractiveBase<TContext, TInput>, IPaginator<TContext, TInput>
		where TContext : IContext
	{
		public virtual async Task PaginateAsync(
			TContext context,
			IPageDisplayer<TContext> displayer,
			IPageOptions<TContext, TInput> options)
		{
			static int Mod(double a, double b)
			{
				if (b == 0)
				{
					return 0;
				}
				return (int)(a - (b * Math.Floor(a / b)));
			}

			var page = options.StartingPage ?? 0;
			var maxPage = options.MaxPage ?? 0;

			while (true)
			{
				await displayer.DisplayAsync(context, page).ConfigureAwait(false);

				var result = await GetPageAsync(context, options).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess || !result.Value.HasValue)
				{
					return;
				}

				page = Mod(page + result.Value.Value, maxPage);
			}
		}

		protected virtual OnInput CreateOnInputDelegate(
			TContext context,
			TaskCompletionSource<int?> eventTrigger,
			IPageOptions<TContext, TInput> options)
		{
			var criteria = options?.Criteria ?? Array.Empty<ICriterion<TContext, TInput>>();

			return new OnInput(async input =>
			{
				foreach (var criterion in criteria)
				{
					var result = await criterion.JudgeAsync(context, input).ConfigureAwait(false);
					if (!result)
					{
						return;
					}
				}

				eventTrigger.SetResult(GetInputValue(input));
			});
		}

		protected abstract int? GetInputValue(TInput input);

		protected virtual async Task<IInteractiveResult<int?>> GetPageAsync(
			TContext context,
			IPageOptions<TContext, TInput> options)
		{
			var eventTrigger = new TaskCompletionSource<int?>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			if (options.Token is CancellationToken token)
			{
				token.Register(() => cancelTrigger.SetResult(true));
			}

			var handler = CreateOnInputDelegate(context, eventTrigger, options);
			// Since delegate equality is kind of wonky, let's just use a guid id in case subscribe
			// creates a new delegate/closure from the OnInput delegate so the dev can store the new
			// delegate/closure in a dictionary and retrieve it to unsubscribe with the correct one
			var id = Guid.NewGuid();
			Subscribe(context, handler, id);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(options.Timeout ?? DefaultTimeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			Unsubscribe(context, handler, id);

			if (task == cancel)
			{
				return new InteractiveResult<int?>(CanceledResult.Instance);
			}
			if (task == delay)
			{
				return new InteractiveResult<int?>(TimedOutResult.Instance);
			}

			var value = await @event.ConfigureAwait(false);
			return new InteractiveResult<int?>(value);
		}
	}
}