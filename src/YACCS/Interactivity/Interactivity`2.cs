using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity
{
	public abstract class Interactivity<TContext, TInput> where TContext : IContext
	{
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected virtual async Task<ITypeReaderResult<TValue>> HandleInteraction<TValue>(
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
			await SubscribeAsync(context, handler).ConfigureAwait(false);
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

		protected abstract Task SubscribeAsync(TContext context, OnInput onInput);

		protected abstract Task UnsubscribeAsync(TContext context, OnInput onInput);

		protected delegate Task<IResult> OnInput(TInput input);
	}
}