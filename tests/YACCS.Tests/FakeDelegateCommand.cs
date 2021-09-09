using YACCS.Commands.Models;

namespace YACCS.Tests
{
	public static class FakeDelegateCommand
	{
		public static DelegateCommand New(Type? contextType = null)
			=> new((Action)(static () => { }), Array.Empty<ImmutablePath>(), contextType);
	}
}