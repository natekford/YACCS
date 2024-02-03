using YACCS.Commands.Models;
using YACCS.Localization;

namespace YACCS.Tests;

public static class FakeDelegateCommand
{
	public static DelegateCommand New(Type? contextType = null)
		=> new(static () => { }, Array.Empty<LocalizedPath>(), contextType);
}