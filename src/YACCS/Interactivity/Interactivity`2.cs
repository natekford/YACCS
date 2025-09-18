using System;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Interactivity;

/// <summary>
/// Gets an <see cref="IResult"/> from an <typeparamref name="TInput"/>.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <param name="input"></param>
/// <returns>The result of this <typeparamref name="TInput"/>.</returns>
public delegate Task<IResult> OnInput<TInput>(TInput input);

/// <summary>
/// The base class for something handling interactions.
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TInput"></typeparam>
public abstract class Interactivity<TContext, TInput> where TContext : IContext
{
	/// <summary>
	/// The amount of time to wait when no timeout is supplied.
	/// </summary>
	protected virtual TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Subscribes <paramref name="handler"/> to some form of input and relies on
	/// <paramref name="handler"/> to set <paramref name="eventTrigger"/> with a value
	/// when the interaction is complete.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="context">
	/// The context which initialized getting input.
	/// </param>
	/// <param name="options">
	/// The interactivity options supplied when this interaction was started.
	/// </param>
	/// <param name="eventTrigger">
	///
	/// </param>
	/// <param name="handler">
	/// Callback for converting <typeparamref name="TInput"/> into
	/// <typeparamref name="TValue"/>.
	/// </param>
	/// <returns>A result indicating success or failure.</returns>
	protected virtual async Task<ITypeReaderResult<TValue>> HandleInteractionAsync<TValue>(
		TContext context,
		InteractivityOptions<TContext, TInput> options,
		TaskCompletionSource<TValue> eventTrigger,
		OnInput<TInput> handler)
	{
		// First up, cancellation registration
		var cancelTrigger = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var cancelRegistration = options.Token is CancellationToken token
			? token.Register(() => cancelTrigger.SetResult(true))
			: default(CancellationTokenRegistration?);
		using var cts = new CancellationTokenSource();

		// Next up, subscribe to the input event
		// This HAS to be disposed
		await using var subscription = await SubscribeAsync(context, handler).ConfigureAwait(false);

		// Now we wait for one of the completion conditions to happen
		var @event = eventTrigger.Task;
		var cancel = cancelTrigger.Task;
		var delay = Task.Delay(options.Timeout ?? DefaultTimeout, cts.Token);
		var task = await Task.WhenAny(@event, delay, cancel).ConfigureAwait(false);

		// Dispose of cancellation
		if (cancelRegistration is not null)
		{
			await cancelRegistration.Value.DisposeAsync().ConfigureAwait(false);
		}
		cts.Cancel();

		if (task == delay)
		{
			return TypeReaderResult<TValue>.TimedOut.Result;
		}
		if (task == cancel)
		{
			return TypeReaderResult<TValue>.Canceled.Result;
		}

		var value = await @event.ConfigureAwait(false);
		return TypeReaders.TypeReaderResult<TValue>.Success(value);
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