#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0022 // Use expression body for methods

using System;

using YACCS.Commands;

namespace YACCS.Tests.Commands
{
	public sealed class FakeContext : IContext
	{
		public Guid Id { get; } = Guid.NewGuid();
		public IServiceProvider Services { get; } = EmptyServiceProvider.Instance;
	}
}