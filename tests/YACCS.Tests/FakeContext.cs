﻿using System;

using YACCS.Commands;

namespace YACCS.Tests
{
	public class FakeContext : IContext
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public IServiceProvider Services { get; set; } = EmptyServiceProvider.Instance;
	}
}