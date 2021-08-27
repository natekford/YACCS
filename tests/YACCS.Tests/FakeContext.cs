using YACCS.Commands;

namespace YACCS.Tests
{
	public class FakeContext : FakeContextBase
	{
	}

	public abstract class FakeContextBase : IContext
	{
		public virtual Guid Id { get; set; } = Guid.NewGuid();
		public virtual IServiceProvider Services { get; set; } = Utils.CreateServices();
		public virtual object Source { get; set; } = null!;
		public virtual DateTime Start { get; set; } = DateTime.UtcNow;
	}

	public class FakeContextChild : FakeContext
	{
	}

	public sealed class OtherContext : FakeContextBase
	{
	}
}