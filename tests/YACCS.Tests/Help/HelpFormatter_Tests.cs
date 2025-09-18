using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Help;

[TestClass]
public class HelpFormatter_Tests
{
	[TestMethod]
	public async Task Format_Test()
	{
		var (commandService, formatter, context) = await CreateAsync().ConfigureAwait(false);

		var command = commandService.Commands.ById(CommandGroup.ID).Single();
		var output = await formatter.FormatAsync(context, command).ConfigureAwait(false);
		const char TRAILING = ' ';

		var expected =
@$"Names: {nameof(CommandGroup.Throws)}
Summary: {CommandGroup.COMMAND_SUMMARY}

Attributes:{TRAILING}
{EnabledByDefaultAttribute.ENABLED_BY_DEFAULT} = {CommandGroup.ENABLED_BY_DEFAULT}
{EnabledByDefaultAttribute.TOGGLEABLE} = {CommandGroup.TOGGLEABLE}
Id = {CommandGroup.ID}
Priority = {CommandGroup.PRIORITY}

Preconditions:{TRAILING}
	{Op.And}
	{CooldownAttribute.OUTPUT}

Parameters:{TRAILING}
	value: integer (-2147483648 to 2147483647)
	Summary: {CommandGroup.PARAMETER_SUMMARY}

	Preconditions:{TRAILING}
		{Op.And}
		{LessThanOrEqualTo100.SUMMARY}

";
		var length = Math.Max(expected.Length, output.Length);
		var sb = new StringBuilder(length);
		for (var i = 0; i < length; ++i)
		{
			Assert.IsTrue(expected.Length > i, "Expected is shorter.");
			Assert.IsTrue(output.Length > i, "Output is shorter.");
			Assert.AreEqual(expected[i], output[i], $"Different characters at index {i}. Value at that point:\n{sb}");
			sb.Append(expected[i]);
		}
	}

	[TestMethod]
	public void FormatterFormattable_Test()
	{
		var formatter = new TagFormatter();
		var formattable = new FormattableJoe();
		_ = formatter.Format("header", formattable, formatter);
		_ = formatter.Format("key", formattable, formatter);
		_ = formatter.Format("value", formattable, formatter);
		Assert.IsFalse(formattable.HasBeenFormatted);
		_ = formatter.Format("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", formattable, formatter);
		Assert.IsTrue(formattable.HasBeenFormatted);
	}

	[TestMethod]
	public void FormatterNoFormatOrFormattable_Test()
		=> Assert.AreEqual("joe", Format<TagFormatter>($"{"joe":idklol}"));

	[TestMethod]
	public void FormatterNull_Test()
		=> Assert.AreEqual(string.Empty, Format<TagFormatter>($"{null}"));

	[TestMethod]
	public void MarkdownTagFormatterHeader_Test()
		=> Assert.AreEqual("**joe**:", Format<MarkdownTagFormatter>($"{"joe":header}"));

	[TestMethod]
	public void MarkdownTagFormatterKey_Test()
		=> Assert.AreEqual("**joe** =", Format<MarkdownTagFormatter>($"{"joe":key}"));

	[TestMethod]
	public void MarkdownTagFormatterValue_Test()
		=> Assert.AreEqual("`joe`", Format<MarkdownTagFormatter>($"{"joe":value}"));

	[TestMethod]
	public void TagFormatterHeader_Test()
		=> Assert.AreEqual("joe:", Format<TagFormatter>($"{"joe":header}"));

	[TestMethod]
	public void TagFormatterKey_Test()
		=> Assert.AreEqual("joe =", Format<TagFormatter>($"{"joe":key}"));

	[TestMethod]
	public void TagFormatterValue_Test()
		=> Assert.AreEqual("joe", Format<TagFormatter>($"{"joe":value}"));

	private static async ValueTask<(FakeCommandService, HelpFormatter, FakeContext)> CreateAsync()
	{
		var context = new FakeContext();
		var formatter = new HelpFormatter(new TypeNameRegistry(), new TagFormatter());
		var commandService = context.Get<FakeCommandService>();
		var commands = typeof(CommandGroup).GetDirectCommandsAsync(context.Services);
		await commandService.AddRangeAsync(commands).ConfigureAwait(false);
		return (commandService, formatter, context);
	}

	private static string Format<T>(FormattableString formattable)
		where T : TagFormatter, new()
	{
		var formatter = new T();
		return formattable.ToString(formatter);
	}

	private class CommandGroup : CommandGroup<IContext>
	{
		public const string COMMAND_SUMMARY = "Throws an exception if the passed in value is greater than 100.";
		public const bool ENABLED_BY_DEFAULT = true;
		public const string ID = "command_id";
		public const string PARAMETER_SUMMARY = "The value to use.";
		public const int PRIORITY = 2;
		public const bool TOGGLEABLE = false;

		[Command(nameof(Throws))]
		[EnabledByDefault(ENABLED_BY_DEFAULT, Toggleable = TOGGLEABLE)]
		[Summary(COMMAND_SUMMARY)]
		[Id(ID)]
		[YACCS.Commands.Attributes.Priority(PRIORITY)]
		[Cooldown]
		public void Throws(
			[LessThanOrEqualTo100]
				[Summary(PARAMETER_SUMMARY)]
				int value)
		{
			if (value > 100)
			{
				throw new InvalidOperationException();
			}
		}
	}

	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	private class CooldownAttribute : Precondition<FakeContext>, IRuntimeFormattableAttribute
	{
		public const string OUTPUT = "ur on cooldown buddy";

		public override async ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			FakeContext context)
		{
			await Task.Delay(250).ConfigureAwait(false);
			return Result.Failure(OUTPUT);
		}

		public async ValueTask<string> FormatAsync(
			IContext context,
			IFormatProvider? formatProvider = null)
		{
			await Task.Delay(250).ConfigureAwait(false);
			return OUTPUT;
		}
	}

	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	private class EnabledByDefaultAttribute(bool enabledByDefault)
		: Attribute, IRuntimeFormattableAttribute
	{
		public const string ENABLED_BY_DEFAULT = "Enabled by default";
		public const string TOGGLEABLE = "Toggleable";

		public bool EnabledByDefault { get; } = enabledByDefault;
		public bool Toggleable { get; set; }

		public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		{
			FormattableString @string = $"{ENABLED_BY_DEFAULT:key} {EnabledByDefault:value}{Environment.NewLine}{TOGGLEABLE:key} {Toggleable:value}";
			return new(@string.ToString(formatProvider));
		}
	}

	private class FormattableJoe : IFormattable
	{
		public bool HasBeenFormatted { get; private set; }

		public string ToString(string? format, IFormatProvider? formatProvider)
		{
			HasBeenFormatted = true;
			return "joe";
		}
	}

	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	[Summary(SUMMARY)]
	private class LessThanOrEqualTo100 : ParameterPrecondition<IContext, int>
	{
		public const string SUMMARY = "The passed in value is less than or equal to 100.";

		protected override ValueTask<IResult> CheckNotNullAsync(
			CommandMeta meta,
			IContext context,
			int value)
		{
			if (value > 100)
			{
				return new(Result.InvalidParameter);
			}
			return new(Result.EmptySuccess);
		}
	}
}