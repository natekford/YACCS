using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace YACCS.Commands
{
	public delegate Task AsyncEventHandler<T>(T e) where T : HandledEventArgs;

	public class AsyncEvent<T> where T : HandledEventArgs
	{
		private readonly Lazy<AsyncEvent<ExceptionEventArgs<T>>> _Exception
			= new Lazy<AsyncEvent<ExceptionEventArgs<T>>>(() => new AsyncEvent<ExceptionEventArgs<T>>());
		private readonly List<AsyncEventHandler<T>> _Handlers
			= new List<AsyncEventHandler<T>>();
		private readonly object _Lock = new object();

		public AsyncEvent<ExceptionEventArgs<T>> Exception => _Exception.Value;

		public void Add(AsyncEventHandler<T> handler)
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

		public async Task InvokeAsync(T e)
		{
			AsyncEventHandler<T>[] handlers;
			lock (_Lock)
			{
				handlers = _Handlers.ToArray();
			}
			if (handlers.Length == 0)
			{
				return;
			}

			var exceptions = new List<Exception>();
			foreach (var handler in handlers)
			{
				try
				{
					var task = handler.Invoke(e);
					if (task != null)
					{
						await task.ConfigureAwait(false);
					}

					if (e.Handled)
					{
						break;
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count > 0)
			{
				var args = new ExceptionEventArgs<T>(exceptions, e);
				await Exception.InvokeAsync(args).ConfigureAwait(false);
			}
		}

		public void Remove(AsyncEventHandler<T> handler)
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
}