using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace YACCS.Commands;

/// <summary>
/// Processes commands in the background.
/// </summary>
public class BackgroundCommandQueue : ICommandQueue
{
	private readonly Channel<Func<Task>> _Channel = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions
	{
		SingleReader = false,
		SingleWriter = false,
	});
	private readonly List<Exception> _Exceptions = [];
	private CancellationTokenSource? _Cts;

	/// <summary>
	/// Whether or not the queue is currently active.
	/// </summary>
	public bool IsRunning => !_Cts?.IsCancellationRequested ?? false;

	/// <inheritdoc/>
	public ValueTask EnqueueAsync(Func<Task> command)
	{
		if (_Exceptions.Count > 0)
		{
			throw new AggregateException("A previous enqueued command has had an exception.", _Exceptions);
		}

		return _Channel.Writer.WriteAsync(command);
	}

	/// <summary>
	/// Starts the queue.
	/// </summary>
	/// <param name="parallelCount">The amount of threads to process the queue with.</param>
	public void Start(int parallelCount)
	{
		if (IsRunning)
		{
			return;
		}

		_Cts?.Cancel();
		var cts = _Cts = new();
		Parallel.For(0, parallelCount, async _ =>
		{
			while (!cts.IsCancellationRequested)
			{
				Func<Task> func;
				try
				{
					func = await _Channel.Reader.ReadAsync(cts.Token).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					break;
				}

				try
				{
					await func.Invoke().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					_Exceptions.Add(e);
				}
			}
		});
	}

	/// <summary>
	/// Stops the queue.
	/// </summary>
	public void Stop()
		=> _Cts?.Cancel();
}