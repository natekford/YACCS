using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandService_Precondition_Tests
	{
		[TestMethod]
		public async Task ProcessPreconditionsFailure_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var command = FakeDelegateCommand.New
				.AsContext<IContext>()
				.AddPrecondition(new FakePrecondition(false))
				.AddPrecondition(new WasIReachedPrecondition())
				.ToCommand();

			var result = await commandService.ProcessPreconditionsAsync(
				new PreconditionCache(context),
				command
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(command.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task ProcessPreconditionsSuccess_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var command = FakeDelegateCommand.New
				.AsContext<IContext>()
				.AddPrecondition(new FakePrecondition(true))
				.AddPrecondition(new WasIReachedPrecondition())
				.ToCommand();

			var result = await commandService.ProcessPreconditionsAsync(
				new PreconditionCache(context),
				command
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(command.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		private class FakePrecondition : Precondition<IContext>
		{
			private readonly bool _Success;

			public FakePrecondition(bool success)
			{
				_Success = success;
			}

			public override Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
				=> _Success ? SuccessResult.InstanceTask : Result.FromError("lol").AsTask();
		}

		private class WasIReachedPrecondition : Precondition<IContext>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}
	}
}