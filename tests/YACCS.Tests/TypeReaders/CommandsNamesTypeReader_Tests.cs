using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class CommandsNamesTypeReader_Tests :
	TypeReader_Tests<IReadOnlyCollection<IImmutableCommand>>
{
	public override ITypeReader<IReadOnlyCollection<IImmutableCommand>> Reader { get; }
		= new CommandsNameTypeReader();

	[TestMethod]
	public async Task Valid_Test()
	{
		var value = await AssertSuccessAsync(new[]
		{
				FakeCommandGroup._Name
			}).ConfigureAwait(false);
		Assert.AreEqual(1, value.Count);
	}

	protected override Task SetupAsync()
	{
		var commandService = Context.Get<CommandService>();
		var commands = typeof(FakeCommandGroup).GetDirectCommandsAsync(Context.Services);
		return commandService.AddRangeAsync(commands);
	}

	private class FakeCommandGroup : CommandGroup<IContext>
	{
		public const string _Name = "joeba";

		[Command(_Name)]
		public void Test()
		{
		}
	}
}