using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public sealed class CommandGroup_Tests
	{
		private readonly IImmutableCommand _Command = FakeDelegateCommand.New().ToImmutable();
		private readonly TestGroup _Concrete = new();
		private readonly FakeContext _Context = new();
		private ICommandGroup Group => _Concrete;

		[TestMethod]
		public void AfterExecution_Test()
		{
			Group.AfterExecutionAsync(_Command, null!, null!);
			Group.AfterExecutionAsync(null!, _Context, null!);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				Group.AfterExecutionAsync(_Command, new InvalidContext(), null!);
			});
		}

		[TestMethod]
		public async Task BeforeExecution_Test()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				Group.BeforeExecutionAsync(_Command, null!);
			});

			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				Group.BeforeExecutionAsync(null!, _Context);
			});

			Assert.ThrowsException<ArgumentException>(() =>
			{
				Group.BeforeExecutionAsync(_Command, new InvalidContext());
			});

			Assert.IsNull(_Concrete.Command);
			Assert.IsNull(_Concrete.Context);
			await Group.BeforeExecutionAsync(_Command, _Context).ConfigureAwait(false);
			Assert.AreSame(_Command, _Concrete.Command);
			Assert.AreSame(_Context, _Concrete.Context);
		}

		private sealed class InvalidContext : IContext
		{
			public Guid Id => throw new NotImplementedException();
			public IServiceProvider Services => throw new NotImplementedException();
			public object Source => throw new NotImplementedException();
		}

		private sealed class TestGroup : CommandGroup<FakeContext>
		{
		}
	}
}