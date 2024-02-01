using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandService_Preconditions_Tests
{
	[TestMethod]
	public async Task ProcessPreconditions_GroupedEndsWithSuccessOR_Test()
	{
		var command = FakeDelegateCommand.New()
			.AsContext<IContext>()
			.AddPrecondition(new FakePrecondition(true)
			{
				Op = Op.And,
			})
			.AddPrecondition(new FakePrecondition(false)
			{
				Op = Op.And,
			})
			.AddPrecondition(new WasIReachedPrecondition())
			.AddPrecondition(new FakePrecondition(true)
			{
				Op = Op.Or,
			})
			.ToImmutable();
		var result = await command.CanExecuteAsync(new FakeContext()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
	}

	[TestMethod]
	public async Task ProcessPreconditions_GroupedIsAllFailureOR_Test()
	{
		var command = FakeDelegateCommand.New()
			.AsContext<IContext>()
			.AddPrecondition(new FakePrecondition(false)
			{
				Op = Op.Or,
			})
			.AddPrecondition(new WasIReachedPrecondition())
			.AddPrecondition(new FakePrecondition(false)
			{
				Op = Op.Or,
			})
			.ToImmutable();
		var result = await command.CanExecuteAsync(new FakeContext()).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
	}

	[TestMethod]
	public async Task ProcessPreconditions_GroupedStartsWithSuccessOR_Test()
	{
		var command = FakeDelegateCommand.New()
			.AsContext<IContext>()
			.AddPrecondition(new FakePrecondition(true)
			{
				Op = Op.Or,
			})
			.AddPrecondition(new WasIReachedPrecondition())
			.ToImmutable();
		var result = await command.CanExecuteAsync(new FakeContext()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
	}

	[TestMethod]
	public async Task ProcessPreconditionsFailure_Test()
	{
		var (context, command) = Create(false);
		var result = await command.CanExecuteAsync(context).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.IsFalse(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
	}

	[TestMethod]
	public async Task ProcessPreconditionsSuccess_Test()
	{
		var (context, command) = Create(true);
		var result = await command.CanExecuteAsync(context).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
	}

	private static (FakeContext, IImmutableCommand) Create(bool success)
	{
		var command = FakeDelegateCommand.New()
			.AsContext<IContext>()
			.AddPrecondition(new FakePrecondition(success))
			.AddPrecondition(new WasIReachedPrecondition())
			.ToImmutable();
		return (new FakeContext(), command);
	}
}