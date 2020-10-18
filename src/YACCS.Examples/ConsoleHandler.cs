using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Examples
{
	public class ConsoleHandler
	{
		private readonly Channel<string?> _Channel;
		private readonly SemaphoreSlim _Input;
		private readonly ITypeRegistry<string> _Names;
		private readonly SemaphoreSlim _Output;

		public ConsoleHandler(ITypeRegistry<string> names)
		{
			_Channel = Channel.CreateUnbounded<string?>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = true,
				AllowSynchronousContinuations = false,
			});
			_ = Task.Run(async () =>
			{
				while (true)
				{
					await _Channel.Writer.WriteAsync(Console.ReadLine()).ConfigureAwait(false);
				}
			});
			_Names = names;
			_Input = new SemaphoreSlim(1, 1);
			_Output = new SemaphoreSlim(1, 1);
		}

		public async Task<string?> ReadLineAsync()
		{
			try
			{
				await _Channel.Reader.WaitToReadAsync().ConfigureAwait(false);
				return await _Channel.Reader.ReadAsync().ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		public void ReleaseBoth()
		{
			ReleaseInput();
			ReleaseOutput();
		}

		public void ReleaseInput()
			=> _Input.ReleaseIfZero();

		public void ReleaseOutput()
			=> _Output.ReleaseIfZero();

		public async Task WaitForBothAsync()
		{
			await _Input.WaitAsync().ConfigureAwait(false);
			await _Output.WaitAsync().ConfigureAwait(false);
		}

		public void WriteLine(string input, ConsoleColor? color = null)
		{
			var newColor = color ?? Console.ForegroundColor;
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = newColor;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}

		public void WriteResult(IResult result)
		{
			var response = result switch
			{
				ParseFailedResult pfr => $"Failed to parse {_Names.Get(pfr.Type)}.",
				_ => result.Response,
			};
			if (!string.IsNullOrWhiteSpace(response))
			{
				WriteLine(response, result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			}
		}
	}
}