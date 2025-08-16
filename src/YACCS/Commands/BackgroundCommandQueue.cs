using System;
using System.Collections.Generic;
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
		SingleWriter = true,
	});
	private readonly List<Exception> _Exceptions = [];

	/// <summary>
	/// Whether or not the queue is currently active.
	/// </summary>
	public bool IsRunning { get; private set; }

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

		IsRunning = true;
		Parallel.For(0, parallelCount, async (_) =>
		{
			while (IsRunning)
			{
				var func = await _Channel.Reader.ReadAsync().ConfigureAwait(false);
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
		=> IsRunning = false;
}