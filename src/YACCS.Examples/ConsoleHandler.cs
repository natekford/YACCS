#pragma warning disable CA1822 // Mark members as static

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using YACCS.Localization;
using YACCS.Results;

namespace YACCS.Examples
{
	public class ConsoleHandler
	{
		private readonly Channel<string?> _Channel;
		private readonly SemaphoreSlim _Input;
		private readonly IReadOnlyDictionary<Type, string> _Names;
		private readonly SemaphoreSlim _Output;

		public ConsoleHandler(IReadOnlyDictionary<Type, string> names)
		{
			_Names = names;
			_Input = new SemaphoreSlim(1, 1);
			_Output = new SemaphoreSlim(1, 1);
			_Channel = Channel.CreateUnbounded<string?>(new UnboundedChannelOptions
			{
				SingleReader = true,
				SingleWriter = true,
				AllowSynchronousContinuations = false,
			});
			_ = Task.Run(async () =>
			{
#if false
				var writer = new TrackingReadLineTextWriter(Console.Out);
				Console.SetOut(writer);
				while (true)
				{
					await _Channel.Writer.WriteAsync(writer.ReadLine()).ConfigureAwait(false);
				}
#else
				while (true)
				{
					await _Channel.Writer.WriteAsync(Console.ReadLine()).ConfigureAwait(false);
				}
#endif
			});
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

		public void ReleaseInputLock()
			=> ReleaseIfZero(_Input);

		public void ReleaseIOLocks()
		{
			ReleaseInputLock();
			ReleaseOutputLock();
		}

		public void ReleaseOutputLock()
			=> ReleaseIfZero(_Output);

		public async Task WaitForBothIOLocksAsync()
		{
			await _Input.WaitAsync().ConfigureAwait(false);
			await _Output.WaitAsync().ConfigureAwait(false);
		}

		public void WriteLine(string input = "", ConsoleColor? color = null)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color ?? Console.ForegroundColor;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}

		public void WriteResult(ICommandResult result)
		{
			var response = FormatResult(result.InnerResult);
			if (!string.IsNullOrWhiteSpace(response))
			{
				if (result.Parameter != null)
				{
					response = result.Parameter.ParameterName + ": " + response;
				}

				WriteLine(response, result.InnerResult.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			}
		}

		public void WriteResult(IResult result)
		{
			var response = FormatResult(result);
			if (!string.IsNullOrWhiteSpace(response))
			{
				WriteLine(response, result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red);
			}
		}

		private string FormatResult(IResult result)
		{
			return result switch
			{
				ParseFailedResult pfr => string.Format(Keys.ParseFailedResult, _Names[pfr.Type]),
				_ => result.Response,
			};
		}

		private static int ReleaseIfZero(SemaphoreSlim semaphore)
			=> semaphore.CurrentCount == 0 ? semaphore.Release() : semaphore.CurrentCount;
	}
}