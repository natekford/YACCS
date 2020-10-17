using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public abstract class Interactivity<TContext, TInput> where TContext : IContext
	{
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected virtual async Task<IInteractivityResult<TValue>> HandleInteraction<TValue>(
			TContext context,
			IInteractivityOptions<TContext, TInput> options,
			Func<TaskCompletionSource<TValue>, OnInput> createHandler)
		{
			var eventTrigger = new TaskCompletionSource<TValue>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			if (options.Token is CancellationToken token)
			{
				token.Register(() => cancelTrigger.SetResult(true));
			}

			var handler = createHandler.Invoke(eventTrigger);
			Subscribe(context, handler);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(options.Timeout ?? DefaultTimeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			Unsubscribe(context, handler);

			if (task == cancel)
			{
				return new InteractivityResult<TValue>(CanceledResult.Instance.Sync);
			}
			if (task == delay)
			{
				return new InteractivityResult<TValue>(TimedOutResult.Instance.Sync);
			}

			var value = await @event.ConfigureAwait(false);
			return new InteractivityResult<TValue>(value);
		}

		protected abstract void Subscribe(TContext context, OnInput onInput);

		protected abstract void Unsubscribe(TContext context, OnInput onInput);

		protected delegate Task<IResult> OnInput(TInput input);
	}
}