using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace YACCS
{
	/// <summary>
	/// Data for when an exception occurs in an <see cref="IAsyncEvent{T}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ExceptionEventArgs<T> : HandledEventArgs where T : HandledEventArgs
	{
		/// <summary>
		/// The event args that were being processed.
		/// </summary>
		public T EventArgs { get; }
		/// <summary>
		/// The exceptions which occurred when processing this event.
		/// </summary>
		public IReadOnlyList<Exception> Exceptions { get; }

		/// <summary>
		/// Creates a new <see cref="ExceptionEventArgs{T}"/>.
		/// </summary>
		/// <param name="exceptions">
		/// <inheritdoc cref="Exceptions" path="/summary"/>
		/// </param>
		/// <param name="eventArgs">
		/// <inheritdoc cref="EventArgs" path="/summary"/>
		/// </param>
		public ExceptionEventArgs(IReadOnlyList<Exception> exceptions, T eventArgs)
		{
			Exceptions = exceptions;
			EventArgs = eventArgs;
		}
	}
}