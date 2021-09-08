using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity
{
	/// <summary>
	/// Gets an <see cref="IResult"/> from an <typeparamref name="TInput"/>.
	/// </summary>
	/// <typeparam name="TInput"></typeparam>
	/// <param name="input"></param>
	/// <returns>The result of this <typeparamref name="TInput"/>.</returns>
	public delegate Task<IResult> OnInput<TInput>(TInput input);

	public abstract class Interactivity<TContext, TInput> where TContext : IContext
	{
		/// <summary>
		/// The amount of time to wait when <see cref="IInteractivityOptions.Timeout"/>
		/// is <see langword="null"/>.
		/// </summary>
		protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

		protected virtual async Task<ITypeReaderResult<TValue>> HandleInteractionAsync<TValue>(
			TContext context,
			IInteractivityOptions<TContext, TInput> options,
			TaskCompletionSource<TValue> eventTrigger,
			OnInput<TInput> handler)
		{
			var cancelTrigger = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var cancelRegistration = options.Token is CancellationToken token
				? token.Register(() => cancelTrigger.SetResult(true))
				: default(CancellationTokenRegistration?);

			using var cts = new CancellationTokenSource();

			var subscription = await SubscribeAsync(context, handler).ConfigureAwait(false);
			var @event = eventTrigger.Task;
			var cancel = cancelTrigger.Task;
			var delay = Task.Delay(options.Timeout ?? DefaultTimeout, cts.Token);
			var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);
			await subscription.DisposeAsync().ConfigureAwait(false);

			if (cancelRegistration is not null)
			{
				await cancelRegistration.Value.DisposeAsync().ConfigureAwait(false);
			}
			cts.Cancel();

			if (task == delay)
			{
				return CachedResults<TValue>.TimedOut.Result;
			}
			if (task == cancel)
			{
				return CachedResults<TValue>.Canceled.Result;
			}

			var value = await @event.ConfigureAwait(false);
			return TypeReaderResult<TValue>.FromSuccess(value);
		}

		/// <summary>
		/// Uses <paramref name="context"/> to subscribe to receive input.
		/// </summary>
		/// <param name="context">
		/// <inheritdoc cref="CommandGroup{TContext}.Context" path="/summary"/>
		/// </param>
		/// <param name="onInput">The callback used to convert input into results.</param>
		/// <returns>A disposable used to cancel the subscription.</returns>
		protected abstract Task<IAsyncDisposable> SubscribeAsync(
			TContext context,
			OnInput<TInput> onInput);
	}
}