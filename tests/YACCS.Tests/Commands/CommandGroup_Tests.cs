using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Tests.Commands;

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

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			Group.AfterExecutionAsync(_Command, new OtherContext(), null!);
		});
	}

	[TestMethod]
	public async Task BeforeExecution_Test()
	{
		Assert.ThrowsExactly<ArgumentNullException>(() =>
		{
			Group.BeforeExecutionAsync(_Command, null!);
		});

		Assert.ThrowsExactly<ArgumentNullException>(() =>
		{
			Group.BeforeExecutionAsync(null!, _Context);
		});

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			Group.BeforeExecutionAsync(_Command, new OtherContext());
		});

		Assert.IsNull(_Concrete.Command);
		Assert.IsNull(_Concrete.Context);
		await Group.BeforeExecutionAsync(_Command, _Context).ConfigureAwait(false);
		Assert.AreSame(_Command, _Concrete.Command);
		Assert.AreSame(_Context, _Concrete.Context);
	}

	private sealed class TestGroup : CommandGroup<FakeContext>;
}