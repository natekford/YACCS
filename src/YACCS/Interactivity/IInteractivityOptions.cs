using System;
using System.Threading;

namespace YACCS.Interactivity
{
	/// <summary>
	/// The base interface for interactivity options.
	/// </summary>
	public interface IInteractivityOptions
	{
		/// <summary>
		/// How long to wait before timing out.
		/// </summary>
		TimeSpan? Timeout { get; }
		/// <summary>
		/// Token used for cancellation.
		/// </summary>
		CancellationToken? Token { get; }
	}
}