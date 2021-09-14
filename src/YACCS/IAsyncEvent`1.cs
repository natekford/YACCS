using System.ComponentModel;

namespace YACCS
{
	/// <summary>
	/// An event which is <see langword="async"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IAsyncEvent<T> where T : HandledEventArgs
	{
		/// <summary>
		/// Fires when an exception occurs in this event.
		/// </summary>
		IAsyncEvent<ExceptionEventArgs<T>> Exception { get; }

		/// <summary>
		/// Adds a handler to this event.
		/// </summary>
		/// <param name="handler"></param>
		void Add(Func<T, Task> handler);

		/// <summary>
		/// Invokes this event.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		Task InvokeAsync(T e);

		/// <summary>
		/// Removes a handler from this event.
		/// </summary>
		/// <param name="handler"></param>
		void Remove(Func<T, Task> handler);
	}
}