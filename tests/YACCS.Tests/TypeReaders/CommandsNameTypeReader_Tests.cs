using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class CommandsNameTypeReader_Tests
	: TypeReader_Tests<IReadOnlyCollection<IImmutableCommand>>
{
	public override ITypeReader<IReadOnlyCollection<IImmutableCommand>> Reader { get; }
		= new CommandsNameTypeReader();

	[TestMethod]
	public async Task Valid_Test()
	{
		var value = await AssertSuccessAsync(
		[
			FakeCommandGroup._Name
		]).ConfigureAwait(false);
		Assert.AreEqual(2, value.Count);
	}

	protected override Task SetupAsync()
	{
		var commandService = Context.Get<FakeCommandService>();
		var commands = typeof(FakeCommandGroup.Joeba).GetDirectCommandsAsync(Context.Services);
		return commandService.AddRangeAsync(commands);
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