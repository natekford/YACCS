using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Interactivity;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public sealed class ConsoleInput : Input<ConsoleContext, string>
	{
		private readonly ConsoleHandler _Console;
		private readonly Dictionary<Guid, CancellationTokenSource> _Input = new();

		public ConsoleInput(
			IReadOnlyDictionary<Type, ITypeReader> readers,
			ConsoleHandler console)
			: base(readers)
		{
			_Console = console;
		}

		protected override string GetInputString(string input)
			=> input;

		protected override async Task SubscribeAsync<TValue>(
			TaskCompletionSource<TValue> task,
			ConsoleContext context,
			OnInput<string, TValue> onInput)
		{
			// Lock both input and output
			// Input because we're using console input
			// Ouput so the next "enter a command to execute" prints after this command is done
			await _Console.WaitForBothIOLocksAsync().ConfigureAwait(false);

			var source = new CancellationTokenSource();
			_Input.Add(context.Id, source);

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

					var result = await onInput.Invoke(task, input).ConfigureAwait(false);
					_Console.WriteResult(result);
				}
			}, source.Token);
		}

		protected override Task UnsubscribeAsync<TValue>(
			ConsoleContext context,
			OnInput<string, TValue> onInput)
		{
			// Only release input lock since output lock gets released when command is done
			_Console.ReleaseInputLock();
			if (_Input.Remove(context.Id, out var token))
			{
				token.Cancel();
			}
			return Task.CompletedTask;
		}
	}
}