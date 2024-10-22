using YACCS.Interactivity;
using YACCS.Results;

namespace YACCS.Examples.Interactivity;

public static class ConsoleSubscription
{
	public static Task<IAsyncDisposable> SubscribeAsync(
		this ConsoleHandler console,
		OnInput<string> onInput)
	{
		var source = new CancellationTokenSource();
		// Run in the background, treat this like an event
		_ = Task.Run(async () =>
		{
			// Only keep the loop going when not canceled
			while (!source.IsCancellationRequested)
			{
				var input = await console.ReadLineAsync(source.Token).ConfigureAwait(false);
				// Even though we have the loop condition already checking this,
				// check it again so we don't invoke the delegate after timeout/cancel
				if (source.IsCancellationRequested)
				{
					return;
				}
				if (input is null)
				{
					continue;
				}

				var result = await onInput.Invoke(input).ConfigureAwait(false);
				if (result is InteractionEnded)
				{
					return;
				}

				// Since it's not InteractionEnded, we can print it out safely
				console.WriteResult(result);
			}
		}, source.Token);

		return Task.FromResult<IAsyncDisposable>(new Subscription(source));
	}

	private sealed class Subscription(CancellationTokenSource source)
		: IAsyncDisposable
	{
		public ValueTask DisposeAsync()
		{
			source.Cancel();
			return new();
		}
	}
}