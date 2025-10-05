using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class CommandsRuntimeIdTypeReader_Tests
	: TypeReader_Tests<IReadOnlyCollection<IImmutableCommand>>
{
	public override ITypeReader<IReadOnlyCollection<IImmutableCommand>> Reader { get; }
		= new CommandsRuntimeIdTypeReader();

	[TestInitialize]
	public override Task SetupAsync()
	{
		RuntimeCommandId.Count = 0;

		var commandService = Context.Get<FakeCommandService>();
		var commands = typeof(FakeCommandGroup.Joeba).GetDirectCommandsAsync(Context.Services);
		return commandService.AddRangeAsync(commands);
	}

	[TestMethod]
	public async Task TooHigh_Test()
		=> await AssertFailureAsync<ParseFailed>(["3"]).ConfigureAwait(false);

	[TestMethod]
	public async Task TooLow_Test()
		=> await AssertFailureAsync<ParseFailed>(["0"]).ConfigureAwait(false);

	[TestMethod]
	public async Task Valid_Test()
	{
		var value = await AssertSuccessAsync(["1"]).ConfigureAwait(false);
		Assert.AreEqual(1, value.Count);
		var value2 = await AssertSuccessAsync(["2"]).ConfigureAwait(false);
		Assert.AreEqual(1, value2.Count);
	}

	private class FakeCommandGroup : CommandGroup<IContext>
	{
		public const string _Name = "joeba";

		[Command(_Name)]
		public class Joeba : CommandGroup<IContext>
		{
			[Command]
			public void Test()
			{
			}

			[Command(nameof(Test2))]
			public void Test2()
			{
			}
		}
	}
}