using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YACCS.Commands;

/// <summary>
/// Processes commands in the background.
/// </summary>
public class BackgroundCommandQueue : ICommandQueue
{
	private readonly List<Exception> _Exceptions = [];
	// This probably should use a Channel instead, but this is simple enough
	// to not bother import the nuget package
	private readonly ConcurrentQueue<Func<Task>> _Queue = [];

	/// <summary>
	/// Whether or not the queue is currently active.
	/// </summary>
	public bool IsRunning { get; private set; }

	/// <inheritdoc/>
	public void Enqueue(Func<Task> command)
	{
		if (_Exceptions.Count > 0)
		{
			throw new AggregateException("A previous enqueued command has had an exception.", _Exceptions);
		}

		_Queue.Enqueue(command);
	}

	/// <summary>
	/// Starts the queue.
	/// </summary>
	/// <param name="parallelCount">The amount of threads to process the queue with.</param>
	/// <param name="queueCheckDelay">Delay between checking the queue.</param>
	public void Start(int parallelCount, TimeSpan? queueCheckDelay = null)
	{
		if (IsRunning)
		{
			return;
		}

		IsRunning = true;
		Parallel.For(0, parallelCount, async (_) =>
		{
			var delay = queueCheckDelay ?? TimeSpan.FromMilliseconds(25);
			while (IsRunning)
			{
				if (_Queue.TryDequeue(out var func))
				{
					try
					{
						await func.Invoke().ConfigureAwait(false);
					}
					catch (Exception e)
					{
						_Exceptions.Add(e);
					}
				}

				await Task.Delay(delay).ConfigureAwait(false);
			}
		});
	}

	/// <summary>
	/// Stops the queue.
	/// </summary>
	public void Stop()
		=> IsRunning = false;
}