﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Preconditions;

[TestClass]
public class Precondition_Tests
{
	[TestMethod]
	public async Task InvalidContext_Test()
	{
		IPrecondition precondition = new FakePrecondition();
		var command = FakeDelegateCommand.New().ToImmutable();
		var context = new OtherContext();

		var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		Assert.AreSame(CachedResults.InvalidContext, result);

		var before = precondition.BeforeExecutionAsync(command, context);
		Assert.IsInstanceOfType<Task<InvalidContext>>(before);

		var after = precondition.AfterExecutionAsync(command, context, null);
		Assert.IsInstanceOfType<Task<InvalidContext>>(after);
	}

	[TestMethod]
	public async Task ValidContext_Test()
	{
		IPrecondition precondition = new FakePrecondition();
		var command = FakeDelegateCommand.New().ToImmutable();
		var context = new FakeContext();

		var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);

		var before = precondition.BeforeExecutionAsync(command, context);
		Assert.AreSame(Task.CompletedTask, before);

		var after = precondition.AfterExecutionAsync(command, context, null);
		Assert.AreSame(Task.CompletedTask, after);
	}

	private class FakePrecondition : Precondition<FakeContext>
	{
		public override ValueTask<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
			=> new(CachedResults.Success);
	}
}