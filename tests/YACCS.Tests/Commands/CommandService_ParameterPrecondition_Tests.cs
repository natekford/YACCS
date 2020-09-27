using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandService_ParameterPrecondition_Tests
	{
		[TestMethod]
		public async Task EnumerableFailure_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameterBuilder = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(DISALLOWED_VALUE))
				.AddParameterPrecondition(new WasIReachedPrecondition());
			var commandBuilder = FakeDelegateCommand.New;
			commandBuilder.Parameters.Add(parameterBuilder);
			var command = commandBuilder.ToCommand();
			var parameter = parameterBuilder.ToParameter();

			var values = new[] { DISALLOWED_VALUE, DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2 };
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				values
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(parameter.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task EnumerableSuccess_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameterBuilder = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(DISALLOWED_VALUE))
				.AddParameterPrecondition(new WasIReachedPrecondition());
			var commandBuilder = FakeDelegateCommand.New;
			commandBuilder.Parameters.Add(parameterBuilder);
			var command = commandBuilder.ToCommand();
			var parameter = parameterBuilder.ToParameter();

			var values = new[] { DISALLOWED_VALUE + 1, DISALLOWED_VALUE + 2, DISALLOWED_VALUE + 3 };
			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				values
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(parameter.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task NonEnumerableFailure_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameterBuilder = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(DISALLOWED_VALUE))
				.AddParameterPrecondition(new WasIReachedPrecondition());
			var commandBuilder = FakeDelegateCommand.New;
			commandBuilder.Parameters.Add(parameterBuilder);
			var command = commandBuilder.ToCommand();
			var parameter = parameterBuilder.ToParameter();

			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				DISALLOWED_VALUE
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsFalse(parameter.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		[TestMethod]
		public async Task NonEnumerableSuccess_Test()
		{
			const int DISALLOWED_VALUE = 1;

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameterBuilder = new Parameter(typeof(int), "")
				.AsType<int>()
				.AddParameterPrecondition(new FakeParameterPrecondition(DISALLOWED_VALUE))
				.AddParameterPrecondition(new WasIReachedPrecondition());
			var commandBuilder = FakeDelegateCommand.New;
			commandBuilder.Parameters.Add(parameterBuilder);
			var command = commandBuilder.ToCommand();
			var parameter = parameterBuilder.ToParameter();

			var result = await commandService.ProcessParameterPreconditionsAsync(
				new PreconditionCache(context),
				command,
				parameter,
				DISALLOWED_VALUE + 1
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsTrue(parameter.Get<WasIReachedPrecondition>().Single().IWasReached);
		}

		private class FakeParameterPrecondition : ParameterPrecondition<IContext, int>
		{
			public int DisallowedValue { get; }

			public FakeParameterPrecondition(int value)
			{
				DisallowedValue = value;
			}

			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] int value)
				=> value == DisallowedValue ? Result.FromError("lol").AsTask() : SuccessResult.InstanceTask;
		}

		private class WasIReachedPrecondition : ParameterPrecondition<IContext, int>
		{
			public bool IWasReached { get; private set; }

			public override Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] int value)
			{
				IWasReached = true;
				return SuccessResult.InstanceTask;
			}
		}
	}
}