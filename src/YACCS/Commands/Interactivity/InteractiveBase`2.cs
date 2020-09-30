using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Interactivity
{
	public abstract class InteractiveBase<TContext, TInput> where TContext : IContext
	{
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected virtual async Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
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
				return new InteractiveResult<TValue>(CanceledResult.Instance);
			}
			if (task == delay)
			{
				return new InteractiveResult<TValue>(TimedOutResult.Instance);
			}

			var value = await @event.ConfigureAwait(false);
			return new InteractiveResult<TValue>(value);
		}

		protected abstract void Subscribe(TContext context, OnInput onInput, Guid id);

		protected abstract void Unsubscribe(TContext context, OnInput onInput, Guid id);

		protected delegate Task OnInput(TInput input);
	}
}