using System;
using System.Threading;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractivityOptions
	{
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
	}
}