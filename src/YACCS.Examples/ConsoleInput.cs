using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Interactivity.Input;
using YACCS.TypeReaders;

namespace YACCS.Examples
{
	public class ConsoleInput : Input<IContext, string>
	{
		private readonly ConsoleHandler _Console;
		private readonly Dictionary<Guid, CancellationTokenSource> _Input = new();

		public ConsoleInput(
			IReadOnlyDictionary<Type, ITypeReader> registry,
			ConsoleHandler console)
			: base(registry)
		{
			_Console = console;
		}

		protected override string GetInputString(string input)
			=> input;

		protected override async Task SubscribeAsync(IContext context, OnInput onInput)
		{
			// Lock both input and output
			// Input because we're using console input
			// Ouput so the next "enter a command to execute" prints after this command is done
			await _Console.WaitForBothIOLocksAsync().ConfigureAwait(false);

			var source = new CancellationTokenSource();
			_ = Task.Run(async () =>
			{
				// Only keep the loop going when not canceled
				while (!source.IsCancellationRequested)
				{
					var input = await _Console.ReadLineAsync().ConfigureAwait(false);
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
					_Console.WriteResult(result);
				}
			});
			_Input.Add(context.Id, source);
		}

		protected override Task UnsubscribeAsync(IContext context, OnInput onInput)
		{
			// Only release input lock since output lock gets released when command is done
			_Console.ReleaseInputLock();
			if (_Input.Remove(context.Id, out var token))
			{
				token.Cancel();
			}
			Console.WriteLine();
			return Task.CompletedTask;
		}
	}
}