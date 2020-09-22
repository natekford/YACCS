using System;

namespace YACCS.Commands
{
	public interface IContext
	{
		public Guid Id { get; }
		public IServiceProvider Services { get; }
	}
}