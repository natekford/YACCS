using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using System.Globalization;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.NamedArguments;
using YACCS.Trie;

namespace YACCS.Tests.Localization;

[TestClass]
public class LocalizedAttributes_Tests
{
	private const string ALIAS = "ali";
	private const string COMMAND = "comm";
	private const string KEY1 = "A";
	private const string KEY2 = "B";
	private const string KEY3 = "C";
	private const string PARAMETER = "dood";
	private const string SUMMARY = "does a thing";
	protected FakeCommandService CommandService { get; set; } = null!;
	protected FakeContext Context { get; set; } = new();
	protected List<string> NotFoundList { get; set; } = [];
	protected NamedArgumentsTypeReader<NamedArgs> Reader { get; set; } = new();

	[TestMethod]
	public void LocalizedCategory_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var attr = new LocalizedCategoryAttribute(KEY1);

		Assert.AreEqual(KEY1, attr.Key);
		Assert.AreEqual(KEY2, attr.Category);

		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.AreEqual(KEY3, attr.Category);
	}

	[TestMethod]
	public async Task LocalizedCommandSearchAfterChangingLocale_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.IsEmpty(CommandService.Commands);

		foreach (var type in new[] { typeof(LocalizedGroup) })
		{
			var commands = type.GetDirectCommandsAsync(Context.Services);
			await CommandService.AddRangeAsync(commands).ConfigureAwait(false);
		}

		var command = CommandService.Commands.Root.FollowPath([nameof(COMMAND)]);
		Assert.IsNotNull(command);
		Assert.IsNotEmpty(NotFoundList);
	}

	[TestMethod]
	public void LocalizedCommandSearchById_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var command = CommandService.Commands.ById(LocalizedGroup.ID).Single();
		Assert.IsNotNull(command);
		Assert.IsEmpty(NotFoundList);
	}

	[TestMethod]
	public void LocalizedCommandSearchByLocalizedPath_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var command = CommandService.Commands.Root.FollowPath([COMMAND]);
		Assert.IsNotNull(command);
		Assert.IsEmpty(NotFoundList);
	}

	[TestMethod]
	public void LocalizedCommandSearchByUnlocalizedPath_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var command = CommandService.Commands.Root.FollowPath([nameof(COMMAND)]);
		Assert.IsNull(command);
		Assert.IsEmpty(NotFoundList);
	}

	[TestMethod]
	public void LocalizedName_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var attr = new LocalizedNameAttribute(KEY1);

		Assert.AreEqual(KEY1, attr.Key);
		Assert.AreEqual(KEY2, attr.Name);

		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		Assert.AreEqual(KEY3, attr.Name);
	}

	[TestMethod]
	public async Task LocalizedTypeReader_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		const int EXPECTED = 73;
		var output = await Reader.ReadAsync(Context, new[] { PARAMETER, EXPECTED.ToString() }).ConfigureAwait(false);
		Assert.IsNotNull(output.Value);
		Assert.AreEqual(EXPECTED, output.Value.D);

		var output2 = await Reader.ReadAsync(Context, new[] { nameof(NamedArgs.D), EXPECTED.ToString() }).ConfigureAwait(false);
		Assert.IsNotNull(output2.Value);
		Assert.AreEqual(EXPECTED, output2.Value.D);
	}

	[TestMethod]
	public async Task LocalizedTypeReaderAfterChangingLocale_Test()
	{
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

		const int EXPECTED = 73;
		var output = await Reader.ReadAsync(Context, new[] { PARAMETER, EXPECTED.ToString() }).ConfigureAwait(false);
		Assert.IsFalse(output.InnerResult.IsSuccess);

		var output2 = await Reader.ReadAsync(Context, new[] { nameof(NamedArgs.D), EXPECTED.ToString() }).ConfigureAwait(false);
		Assert.IsNotNull(output2.Value);
		Assert.AreEqual(EXPECTED, output2.Value.D);
	}

	[TestCleanup]
	public void TestCleanup()
		=> CultureInfo.CurrentUICulture = CultureInfo.InstalledUICulture;

	[TestInitialize]
	public async Task TestInitializeAsync()
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

		var localizer = new RuntimeLocalizer();
		localizer.Overrides[CultureInfo.CurrentUICulture].Add(KEY1, KEY2);
		localizer.Overrides[CultureInfo.InvariantCulture].Add(KEY1, KEY3);

		localizer.Overrides[CultureInfo.CurrentUICulture].Add(nameof(COMMAND), COMMAND);
		localizer.Overrides[CultureInfo.CurrentUICulture].Add(nameof(ALIAS), ALIAS);
		localizer.Overrides[CultureInfo.CurrentUICulture].Add(nameof(SUMMARY), SUMMARY);
		localizer.Overrides[CultureInfo.CurrentUICulture].Add(nameof(PARAMETER), PARAMETER);
		Localize.Instance.Append(localizer);

		Context = new FakeContext();
		CommandService = Context.Get<FakeCommandService>();
		foreach (var type in new[] { typeof(LocalizedGroup) })
		{
			var commands = type.GetDirectCommandsAsync(Context.Services);
			await CommandService.AddRangeAsync(commands).ConfigureAwait(false);
		}

		Localize.Instance.KeyNotFound += (key, culture) =>
		{
			var msg = $"Unable to find '{key}' in '{culture}'.";
			NotFoundList.Add(msg);
		};
	}

	public sealed class LocalizedGroup : CommandGroup<IContext>
	{
		public const string ID = "localized_id";

		[Command(nameof(COMMAND), nameof(ALIAS))]
		[LocalizedSummary(nameof(SUMMARY))]
		[Id(ID)]
		public void DoThing(NamedArgs args)
		{
		}
	}

	[GenerateNamedArguments]
	public sealed class NamedArgs
	{
		[LocalizedName(nameof(PARAMETER))]
		public int D { get; set; }
	}
}