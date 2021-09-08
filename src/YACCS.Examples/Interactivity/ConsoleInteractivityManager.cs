using YACCS.Interactivity;
using YACCS.Results;

namespace YACCS.Examples.Interactivity
{
	public sealed class ConsoleInteractivityManager
	{
		private readonly ConsoleHandler _Console;

		public ConsoleInteractivityManager(ConsoleHandler console)
		{
			_Console = console;
		}

		public async Task<IAsyncDisposable> SubscribeAsync(OnInput<string> onInput)
		{
			// Lock both input and output
			// Input because we're using console input
			// Ouput so the next "enter a command to execute" prints after this command is done
			await _Console.WaitForBothIOLocksAsync().ConfigureAwait(false);

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
					if (result is InteractionEndedResult)
					{
						return;
					}

					// Since it's not InteractionEnded, we can print it out safely
					_Console.WriteResult(result);
				}
			}, source.Token);

			return new ConsoleSubscription(_Console, source);
		}

		private sealed class ConsoleSubscription : IAsyncDisposable
		{
			private readonly ConsoleHandler _Console;
			private readonly CancellationTokenSource _Source;

			public ConsoleSubscription(ConsoleHandler console, CancellationTokenSource source)
			{
				_Console = console;
				_Source = source;
			}

			public ValueTask DisposeAsync()
			{
				// Only release input lock since output lock gets released when command is done
				_Console.ReleaseInputLock();
				_Source.Cancel();
				return new();
			}
		}
	}
}