using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

[TestClass]
public class CommandsCategoriesTypeReader_Tests
	: TypeReader_Tests<IReadOnlyCollection<IImmutableCommand>>
{
	public override ITypeReader<IReadOnlyCollection<IImmutableCommand>> Reader { get; }
		= new CommandsCategoryTypeReader();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync<ParseFailedResult>(Array.Empty<string>()).ConfigureAwait(false);

	[TestMethod]
	public async Task OneCategory_Test()
	{
		var value = await AssertSuccessAsync(new[]
		{
				FakeCommandGroup._Category1
			}).ConfigureAwait(false);
		Assert.AreEqual(2, value.Count);
	}

	[TestMethod]
	public async Task TwoCategories_Test()
	{
		var value = await AssertSuccessAsync(new[]
		{
				FakeCommandGroup._Category1,
				FakeCommandGroup._Category2
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
		public const string _Category1 = "steven";
		public const string _Category2 = "bob";
		public const string _Name = "joeba";

		[Command(_Name)]
		[Category(_Category1)]
		[Category(_Category2)]
		public void Test1()
		{
		}

		[Command(_Name)]
		[Category(_Category1)]
		public void Test2()
		{
		}
	}
}
