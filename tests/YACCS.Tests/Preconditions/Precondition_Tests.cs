using System;
using System.Linq;
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
			var command = FakeDelegateCommand.New().ToImmutable().Single();
			var context = new FakeContext2();

			var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(InvalidContextResult));

			var after = precondition.AfterExecutionAsync(command, context);
			Assert.AreEqual(InvalidContextResult.Instance.Task, after);
		}

		[TestMethod]
		public async Task ValidContext_Test()
		{
			var precondition = new FakePrecondition();
			var command = FakeDelegateCommand.New().ToImmutable().Single();
			var context = new FakeContext();

			var result = await precondition.CheckAsync(command, context).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);

			var after = precondition.AfterExecutionAsync(command, context);
			Assert.AreEqual(Task.CompletedTask, after);

			var precondition2 = ((IPrecondition)precondition);
			var result2 = await precondition2.CheckAsync(command, context).ConfigureAwait(false);
			Assert.IsTrue(result2.IsSuccess);

			var after2 = precondition2.AfterExecutionAsync(command, context);
			Assert.AreEqual(Task.CompletedTask, after2);
		}

		private class FakeContext2 : IContext
		{
			public Guid Id => throw new NotImplementedException();
			public IServiceProvider Services => throw new NotImplementedException();
		}

		private class FakePrecondition : Precondition<FakeContext>
		{
			public override Task<IResult> CheckAsync(IImmutableCommand command, FakeContext context)
				=> SuccessResult.Instance.Task;
		}
	}
}