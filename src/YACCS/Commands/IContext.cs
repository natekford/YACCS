using System;

namespace YACCS.Commands
{
	public interface IContext
	{
		Guid Id { get; }
		IServiceProvider Services { get; }
		object Source { get; }
		DateTime Start { get; }
	}
}