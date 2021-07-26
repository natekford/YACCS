using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Interactivity;
using YACCS.Results;

namespace YACCS.Examples.Interactivity
{
	public sealed class ConsoleInteractivityManager
	{
		private readonly Dictionary<Guid, CancellationTokenSource> _Input = new();

		public ConsoleHandler Console { get; }

		public ConsoleInteractivityManager(ConsoleHandler console)
		{
			Console = console;
		}

		public async Task SubscribeAsync(ConsoleContext context, OnInput<string> onInput)
		{
			// Lock both input and output
			// Input because we're using console input
			// Ouput so the next "enter a command to execute" prints after this command is done
			await Console.WaitForBothIOLocksAsync().ConfigureAwait(false);

			var source = new CancellationTokenSource();
			_Input.Add(context.Id, source);

			// Run in the background, treat this like an event
			_ = Task.Run(async () =>
			{
				// Only keep the loop going when not canceled
				while (!source.IsCancellationRequested)
				{
					var input = await Console.ReadLineAsync(source.Token).ConfigureAwait(false);
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
					// If interaction is ended, the locks have been released and
					// if we print out the result, it will be out of order
					if (result is not InteractionEndedResult)
					{
						Console.WriteResult(result);
					}
				}
			}, source.Token);
		}

		public Task UnsubscribeAsync(ConsoleContext context, OnInput<string> _)
		{
			// Only release input lock since output lock gets released when command is done
			Console.ReleaseInputLock();
			if (_Input.Remove(context.Id, out var token))
			{
				token.Cancel();
			}
			return Task.CompletedTask;
		}
	}
}