#pragma warning disable CA1822 // Mark members as static

using System.Threading.Channels;

using YACCS.Results;

namespace YACCS.Examples
{
	public sealed class ConsoleHandler
	{
		private readonly Channel<string?> _Channel;
		private readonly IFormatProvider _FormatProvider;
		private readonly SemaphoreSlim _IOLock;

		public ConsoleHandler(IFormatProvider formatProvider)
		{
			_FormatProvider = formatProvider;
			_IOLock = new SemaphoreSlim(1, 1);
			_Channel = Channel.CreateUnbounded<string?>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = true,
				AllowSynchronousContinuations = false,
			});
			_ = Task.Run(StartWritingToChannel);
		}

		private async Task StartWritingToChannel()
		{
			while (true)
			{
				await _Channel.Writer.WriteAsync(Console.ReadLine()).ConfigureAwait(false);
			}
		}

		public async Task<string?> ReadLineAsync(CancellationToken token = default)
		{
			try
			{
				return await _Channel.Reader.ReadAsync(token).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		public void ReleaseIOLock()
			=> ReleaseIfZero(_IOLock);

		public Task WaitForIOLockAsync(CancellationToken token = default)
			=> _IOLock.WaitAsync(token);

		public void WriteLine(string input = "", ConsoleColor? color = null)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color ?? Console.ForegroundColor;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}

		public void WriteResult(IExecuteResult result)
		{
			var response = FormatResult(result.InnerResult);
			if (!string.IsNullOrWhiteSpace(response))
			{
				if (result.Parameter is not null)
				{
					response = $"{result.Parameter.ParameterName}: {response}";
				}

				WriteLine(response, result.InnerResult.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
				Console.WriteLine();
			}
		}

		public void WriteResult(IResult result)
		{
			var response = FormatResult(result);
			if (!string.IsNullOrWhiteSpace(response))
			{
				WriteLine(response, result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
				Console.WriteLine();
			}
		}

		private string FormatResult(IResult result)
		{
			return result switch
			{
				IFormattable formattable => formattable.ToString(null, _FormatProvider),
				_ => result.Response,
			};
		}

		private static int ReleaseIfZero(SemaphoreSlim semaphore)
			=> semaphore.CurrentCount == 0 ? semaphore.Release() : semaphore.CurrentCount;
	}
}