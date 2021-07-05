using System;

namespace YACCS
{
	public sealed class EmptyServiceProvider : IServiceProvider
	{
		public static EmptyServiceProvider Instance { get; } = new();

		private EmptyServiceProvider()
		{
		}

		public object? GetService(Type serviceType) => null;
	}
}