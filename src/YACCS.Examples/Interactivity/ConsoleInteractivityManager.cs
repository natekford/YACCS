using YACCS.Interactivity;
using YACCS.Results;

namespace YACCS.Examples.Interactivity;

public sealed class ConsoleInteractivityManager
{
	private readonly ConsoleHandler _Console;

	public ConsoleInteractivityManager(ConsoleHandler console)
	{
		_Console = console;
	}

	public Task<IAsyncDisposable> SubscribeAsync(OnInput<string> onInput)
	{
		var source = new CancellationTokenSource();
		// Run in the background, treat this like an event
		_ = Task.Run(async () =>
		{
			// Only keep the loop going when not canceled
			while (!source.IsCancellationRequested)
			{
				var input = await _Console.ReadLineAsync(source.Token).ConfigureAwait(false);
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
				_Console.WriteResult(result);
			}
		}, source.Token);

		var subscription = new ConsoleSubscription(source);
		return Task.FromResult<IAsyncDisposable>(subscription);
	}

	private sealed class ConsoleSubscription : IAsyncDisposable
	{
		private readonly CancellationTokenSource _Source;

		public ConsoleSubscription(CancellationTokenSource source)
		{
			_Source = source;
		}

		public ValueTask DisposeAsync()
		{
			_Source.Cancel();
			return new();
		}
	}
}