using System;

using YACCS.Commands.Models;

namespace YACCS.Tests
{
	public static class FakeDelegateCommand
	{
		public static DelegateCommand New(Type? type = null)
			=> new((Action)(() => { }), Array.Empty<ImmutableName>(), type);
	}
}