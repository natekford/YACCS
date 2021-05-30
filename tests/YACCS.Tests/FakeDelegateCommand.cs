using System;

using YACCS.Commands.Models;

namespace YACCS.Tests
{
	public static class FakeDelegateCommand
	{
		public static DelegateCommand New(Type? type = null)
		{
			var @delegate = (Action)(() => { });
			return new DelegateCommand(@delegate, type, Array.Empty<Name>());
		}
	}
}