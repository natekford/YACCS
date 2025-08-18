using System.Threading.Channels;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Sample;

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
		_ = Task.Run(async () =>
		{
			while (true)
			{
				await _Channel.Writer.WriteAsync(Console.ReadLine()).ConfigureAwait(false);
			}
		});
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

	public async Task WaitForIOLockAsync(CancellationToken token = default)
		=> await _IOLock.WaitAsync(token).ConfigureAwait(false);

#pragma warning disable CA1822 // Mark members as static

	public void WriteLine(string input = "", ConsoleColor? color = null)
#pragma warning restore CA1822 // Mark members as static
	{
		var oldColor = Console.ForegroundColor;
		Console.ForegroundColor = color ?? Console.ForegroundColor;
		Console.WriteLine(input);
		Console.ForegroundColor = oldColor;
	}

	public void WriteResult(CommandScore result)
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

	private static int ReleaseIfZero(SemaphoreSlim semaphore)
		=> semaphore.CurrentCount == 0 ? semaphore.Release() : semaphore.CurrentCount;

	private string FormatResult(IResult result)
	{
		return result switch
		{
			IFormattable formattable => formattable.ToString(null, _FormatProvider),
			_ => result.Response,
		};
	}
}