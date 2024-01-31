using System.ComponentModel;

namespace YACCS;

/// <inheritdoc cref="IAsyncEvent{T}"/>.
public class AsyncEvent<T> : IAsyncEvent<T> where T : HandledEventArgs
{
	private readonly Lazy<AsyncEvent<ExceptionEventArgs<T>>> _Exception
		= new(static () => new());
	private readonly List<Func<T, Task>> _Handlers = [];
	private readonly object _Lock = new();

	/// <inheritdoc />
	public IAsyncEvent<ExceptionEventArgs<T>> Exception => _Exception.Value;

	/// <inheritdoc />
	public void Add(Func<T, Task> handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}
		lock (_Lock)
		{
			_Handlers.Add(handler);
		}
	}

	/// <inheritdoc />
	public async Task InvokeAsync(T args)
	{
		Func<T, Task>[] handlers;
		lock (_Lock)
		{
			handlers = [.. _Handlers];
		}
		if (handlers.Length == 0)
		{
			return;
		}

		var exceptions = default(List<Exception>);
		foreach (var handler in handlers)
		{
			try
			{
				var task = handler.Invoke(args);
				if (task is not null)
				{
					await task.ConfigureAwait(false);
				}

				if (args.Handled)
				{
					break;
				}
			}
			catch (Exception e)
			{
				exceptions ??= [];
				exceptions.Add(e);
			}
		}

		if (exceptions is not null && _Exception.IsValueCreated)
		{
			var exceptionArgs = new ExceptionEventArgs<T>(exceptions, args);
			await Exception.InvokeAsync(exceptionArgs).ConfigureAwait(false);
		}
	}

	/// <inheritdoc />
	public void Remove(Func<T, Task> handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}
		lock (_Lock)
		{
			_Handlers.Remove(handler);
		}
	}
}