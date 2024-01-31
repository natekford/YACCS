using System.ComponentModel;

namespace YACCS;

/// <summary>
/// Data for when an exception occurs in an <see cref="IAsyncEvent{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates a new <see cref="ExceptionEventArgs{T}"/>.
/// </remarks>
/// <param name="exceptions">
/// <inheritdoc cref="Exceptions" path="/summary"/>
/// </param>
/// <param name="eventArgs">
/// <inheritdoc cref="EventArgs" path="/summary"/>
/// </param>
public class ExceptionEventArgs<T>(
	IReadOnlyList<Exception> exceptions,
	T eventArgs)
	: HandledEventArgs where T : HandledEventArgs
{
	/// <summary>
	/// The event args that were being processed.
	/// </summary>
	public T EventArgs { get; } = eventArgs;
	/// <summary>
	/// The exceptions which occurred when processing this event.
	/// </summary>
	public IReadOnlyList<Exception> Exceptions { get; } = exceptions;
}