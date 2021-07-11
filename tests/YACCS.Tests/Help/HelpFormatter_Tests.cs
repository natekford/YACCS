using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

using PriorityAttribute = YACCS.Commands.Attributes.PriorityAttribute;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class HelpFormatter_Tests
	{
		[TestMethod]
		public async Task Format_Test()
		{
			var (commands, formatter, context) = await CreateAsync().ConfigureAwait(false);

			var command = commands.ById(CommandGroup.ID).Single();
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
{CooldownAttribute.OUTPUT}

Parameters:{TRAILING}
value: integer (-2147483648 to 2147483647)
	Summary: {CommandGroup.PARAMETER_SUMMARY}

	Preconditions:{TRAILING}
	{LessThanOrEqualTo100Attribute.SUMMARY}

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

		private static async Task<(IEnumerable<IImmutableCommand>, IHelpFormatter, IContext)> CreateAsync()
		{
			var commands = await typeof(CommandGroup).GetDirectCommandsAsync().ConfigureAwait(false);
			var formatter = new HelpFormatter(new TypeNameRegistry(), new TagFormatter());
			var context = new FakeContext();
			return (commands, formatter, context);
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
			[Priority(PRIORITY)]
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
		private class CooldownAttribute : PreconditionAttribute, IRuntimeFormattableAttribute
		{
			public const string OUTPUT = "ur on cooldown buddy";

			public override async Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
			{
				await Task.Delay(250).ConfigureAwait(false);
				return new FailureResult(OUTPUT);
			}

			public async ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			{
				await Task.Delay(250).ConfigureAwait(false);
				return OUTPUT;
			}
		}

		[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
		private class EnabledByDefaultAttribute : Attribute, IRuntimeFormattableAttribute
		{
			public const string ENABLED_BY_DEFAULT = "Enabled by default";
			public const string TOGGLEABLE = "Toggleable";

			public bool EnabledByDefault { get; }
			public bool Toggleable { get; set; }

			public EnabledByDefaultAttribute(bool enabledByDefault)
			{
				EnabledByDefault = enabledByDefault;
			}

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
		private class LessThanOrEqualTo100Attribute : ParameterPreconditionAttribute
		{
			public const string SUMMARY = "The passed in value is less than or equal to 100.";

			protected override Task<IResult> CheckAsync(
				IImmutableCommand command,
				IImmutableParameter parameter,
				IContext context,
				object? value)
			{
				return this.CheckAsync<IContext, int>(command, parameter, context, value, (cm, p, c, v) =>
				{
					if (v > 100)
					{
						return InvalidParameterResult.Instance.Task;
					}
					return SuccessResult.Instance.Task;
				});
			}
		}
	}
}