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
			IPageOptions<TContext, TInput> options)
		{
			var display = await CreateDisplayAsync(context).ConfigureAwait(false);

			var page = options.StartingPage ?? 0;
			while (true)
			{
				var result = await GetPageDifferenceAsync(context, options).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess || !result.Value.HasValue)
				{
					return;
				}

				page += result.Value.Value;
				if (page < 0)
				{
					page += options.MaxPage ?? 0;
				}
				else if (page >= options.MaxPage)
				{
					page -= options.MaxPage ?? 0;
				}

				await UpdateDisplayAsync(context, display, page).ConfigureAwait(false);
			}
		}

		protected abstract Task<TDisplay> CreateDisplayAsync(TContext context);

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

		protected virtual async Task<IInteractiveResult<int?>> GetPageDifferenceAsync(
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

		protected abstract Task UpdateDisplayAsync(TContext context, TDisplay display, int page);
	}
}