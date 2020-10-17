using System;
using System.Collections.Generic;
using System.Threading;

namespace YACCS.Commands.Interactivity
{
	public interface IInteractivityOptions
	{
		TimeSpan? Timeout { get; }
		CancellationToken? Token { get; }
	}
}