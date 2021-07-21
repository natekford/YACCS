using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity
{
	public delegate Task<IResult> OnInput<TInput, TValue>(
		TaskCompletionSource<TValue> task,
		TInput input);

	public abstract class Interactivity<TContext, TInput> where TContext : IContext
	{
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected virtual async Task<ITypeReaderResult<TValue>> HandleInteraction<TValue>(
			TContext context,
			IInteractivityOptions<TContext, TInput> options,
			OnInput<TInput, TValue> handler)
		{
			var eventTrigger = new TaskCompletionSource<TValue>();
			var cancelTrigger = new TaskCompletionSource<bool>();
			if (options.Token is CancellationToken token)
			{
				token.Register(() => cancelTrigger.SetResult(true));
			}

			await SubscribeAsync(eventTrigger, context, handler).ConfigureAwait(false);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(options.Timeout ?? DefaultTimeout);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			await UnsubscribeAsync(context, handler).ConfigureAwait(false);

			if (task == cancel)
			{
				return CachedResults<TValue>.Canceled;
			}
			if (task == delay)
			{
				return CachedResults<TValue>.TimedOut;
			}

			var value = await @event.ConfigureAwait(false);
			return TypeReaderResult<TValue>.FromSuccess(value);
		}

		protected abstract Task SubscribeAsync<TValue>(
			TaskCompletionSource<TValue> task,
			TContext context,
			OnInput<TInput, TValue> onInput);

		protected abstract Task UnsubscribeAsync<TValue>(
			TContext context,
			OnInput<TInput, TValue> onInput);
	}
}