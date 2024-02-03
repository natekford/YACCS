using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Results;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandService_GetBestMatchAsync_Tests
{
	private const int DISALLOWED_VALUE = 1;

	[TestMethod]
	public async Task FailedDefaultValue_Test()
	{
		var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
		var result = await commandService.ProcessAllPreconditionsAsync(
			context,
			command.ToImmutable(),
			new[] { DISALLOWED_VALUE.ToString() },
			1
		).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.FailedTypeReader, result.Stage);
		Assert.AreEqual(1, result.Index);

		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task FailedParameterPrecondition_Test()
	{
		var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
		var result = await commandService.ProcessAllPreconditionsAsync(
			context,
			command.ToImmutable(),
			new[] { DISALLOWED_VALUE.ToString() },
			0
		).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.FailedParameterPrecondition, result.Stage);
		Assert.AreEqual(1, result.Index);

		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task FailedPrecondition_Test()
	{
		var (commandService, context, command, parameter) = Create(false, DISALLOWED_VALUE);
		var result = await commandService.ProcessAllPreconditionsAsync(
			context,
			command.ToImmutable(),
			new[] { DISALLOWED_VALUE.ToString() },
			0
		).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.FailedPrecondition, result.Stage);
		Assert.AreEqual(0, result.Index);

		Assert.IsFalse(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task FailedTypeReader_Test()
	{
		var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
		var result = await commandService.ProcessAllPreconditionsAsync(
			context,
			command.ToImmutable(),
			new[] { "joeba" },
			0
		).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.FailedTypeReader, result.Stage);
		Assert.AreEqual(0, result.Index);

		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
		Assert.IsFalse(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	[TestMethod]
	public async Task InvalidContext_Test()
	{
		var (commandService, _, command, _) = Create(true, DISALLOWED_VALUE);
		var score = await commandService.GetCommandScoreAsync(
			new OtherContext(),
			command.ToImmutable(),
			new[] { "a" },
			0
		).ConfigureAwait(false);
		Assert.IsFalse(score.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.BadContext, score.Stage);
	}

	[TestMethod]
	public async Task InvalidLength_Test()
	{
		var (commandService, context, command, _) = Create(true, DISALLOWED_VALUE);

		// Not enough
		{
			var score = await commandService.GetCommandScoreAsync(
				context,
				command.ToImmutable(),
				Array.Empty<string>(),
				0
			).ConfigureAwait(false);
			Assert.IsFalse(score.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.BadArgCount, score.Stage);
			Assert.IsInstanceOfType(score.InnerResult, typeof(NotEnoughArgs));
		}

		// Too many
		{
			var score = await commandService.GetCommandScoreAsync(
				context,
				command.ToImmutable(),
				new[] { "a", "b", "c", "d", "e", "f" },
				0
			).ConfigureAwait(false);
			Assert.IsFalse(score.InnerResult.IsSuccess);
			Assert.AreEqual(CommandStage.BadArgCount, score.Stage);
			Assert.IsInstanceOfType(score.InnerResult, typeof(TooManyArgs));
		}
	}

	[TestMethod]
	public async Task Successful_Test()
	{
		var (commandService, context, command, parameter) = Create(true, DISALLOWED_VALUE);
		var result = await commandService.ProcessAllPreconditionsAsync(
			context,
			command.ToImmutable(),
			new[] { (DISALLOWED_VALUE + 1).ToString() },
			0
		).ConfigureAwait(false);

		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandStage.CanExecute, result.Stage);
		Assert.AreEqual(1, result.Index);

		Assert.IsTrue(command.GetAttributes<WasIReachedPrecondition>().Single().IWasReached);
		Assert.IsTrue(parameter.GetAttributes<WasIReachedParameterPreconditionAttribute>().Single().IWasReached);
	}

	private static (CommandService, FakeContext, IMutableCommand, IMutableParameter) Create(bool success, int disallowedValue)
	{
		static void Delegate(int arg)
		{
		}

		var commandBuilder = new DelegateCommand(Delegate, Array.Empty<LocalizedPath>(), typeof(FakeContext))
			.AsContext<FakeContext>()
			.AddPrecondition(new FakePrecondition(success))
			.AddPrecondition(new WasIReachedPrecondition());
		commandBuilder.Parameters[0]
			.AsType<int>()
			.AddParameterPrecondition(new FakeParameterPreconditionAttribute(disallowedValue))
			.AddParameterPrecondition(new WasIReachedParameterPreconditionAttribute());

		var context = new FakeContext();
		return (context.Get<CommandService>(), context, commandBuilder, commandBuilder.Parameters[0]);
	}
}
