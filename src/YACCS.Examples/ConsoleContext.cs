using System;

using YACCS.Commands;

namespace YACCS.Examples
{
	public class ConsoleContext : IContext
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string Input { get; }
		public IServiceProvider Services { get; }

		public ConsoleContext(IServiceProvider services, string input)
		{
			Services = services;
			Input = input;
		}
	}
}