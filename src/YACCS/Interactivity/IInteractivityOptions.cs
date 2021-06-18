using System;
using System.Threading;

namespace YACCS.Interactivity
{
	public interface IInteractivityOptions
	{
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
	}
}