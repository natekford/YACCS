using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Preconditions
{
	[TestClass]
	public class Precondition_Tests
	{
		[TestMethod]
		public async Task InvalidContext_Test()
		{
			IPrecondition precondition = new FakePrecondition();
			var command = FakeDelegateCommand.New().MakeImmutable();
			var context = new FakeContext2();

			var before = precondition.BeforeExecutionAsync(command, context);
			Assert.AreEqual(InvalidContextResult.Instance.Task, before.AsTask());

			var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(InvalidContextResult));

			var after = precondition.AfterExecutionAsync(command, context, null);
			Assert.AreEqual(InvalidContextResult.Instance.Task, after.AsTask());
		}

		[TestMethod]
		public async Task ValidContext_Test()
		{
			IPrecondition precondition = new FakePrecondition();
			var command = FakeDelegateCommand.New().MakeImmutable();
			var context = new FakeContext();

			var before = precondition.BeforeExecutionAsync(command, context);
			Assert.AreEqual(ValueTask.CompletedTask, before);

			var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			var after = precondition.AfterExecutionAsync(command, context, null);
			Assert.AreEqual(ValueTask.CompletedTask, after);
		}

		private class FakeContext2 : IContext
		{
			public Guid Id => throw new NotImplementedException();
			public IServiceProvider Services => throw new NotImplementedException();
		}

		private class FakePrecondition : Precondition<FakeContext>
		{
			public override ValueTask<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
				=> new(SuccessResult.Instance.Sync);
		}
	}
}