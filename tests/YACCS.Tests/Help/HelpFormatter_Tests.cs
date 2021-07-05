using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

using PriorityAttribute = YACCS.Commands.Attributes.PriorityAttribute;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class HelpFormatter_Tests
	{
		[TestMethod]
		public async Task Async_Test()
		{
			var (commands, formatter, context) = await CreateAsync().ConfigureAwait(false);

			var command = commands.ById(CommandGroup.ASYNC).Single();
			var task = formatter.FormatAsync(context, command);
			Assert.IsFalse(task.IsCompleted);
			Assert.IsFalse(task.IsCompletedSuccessfully);
			await task.ConfigureAwait(false);
			Debug.WriteLine(task.Result);
		}

		[TestMethod]
		public async Task Sync_Test()
		{
			var (commands, formatter, context) = await CreateAsync().ConfigureAwait(false);

			var command = commands.ById(CommandGroup.SYNC).Single();
			var task = formatter.FormatAsync(context, command);
			Assert.IsTrue(task.IsCompleted);
			Assert.IsTrue(task.IsCompletedSuccessfully);
			Debug.WriteLine(task.Result);
		}

		private static async Task<(IEnumerable<IImmutableCommand>, IHelpFormatter, IContext)> CreateAsync()
		{
			var commands = await typeof(CommandGroup).GetDirectCommandsAsync().ConfigureAwait(false);
			var formatter = new HelpFormatter(new TypeNameRegistry(), new TagConverter(Localize.Instance));
			var context = new FakeContext();
			return (commands, formatter, context);
		}

		private class CommandGroup : CommandGroup<IContext>
		{
			public const string ASYNC = "async_id";
			public const string SYNC = "sync_id";

			[Command(nameof(Async))]
			[EnabledByDefault(true, Toggleable = false)]
			[Summary("Throws an exception if the passed in value is greater than 100.")]
			[Id(ASYNC)]
			[Priority(2)]
			[Cooldown]
			public void Async(
				[LessThanOrEqualTo100]
				[Summary("The value to use.")]
				int value)
			{
				if (value > 100)
				{
					throw new InvalidOperationException();
				}
			}

			[Command(nameof(Sync))]
			[EnabledByDefault(true, Toggleable = false)]
			[Summary("Throws an exception if the passed in value is greater than 100.")]
			[Id(SYNC)]
			[Priority(1)]
			public void Sync(
				[LessThanOrEqualTo100]
				[Summary("The value to use.")]
				int value)
			{
				if (value > 100)
				{
					throw new InvalidOperationException();
				}
			}
		}

		[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
		private class CooldownAttribute : PreconditionAttribute, IAsyncRuntimeFormattableAttribute
		{
			private static Task<bool> ADatabaseCall => Task.Run(async () =>
			{
				await Task.Delay(250).ConfigureAwait(false);
				return new Random().NextDouble() >= 0.5;
			});

			public override async Task<IResult> CheckAsync(IImmutableCommand command, IContext context)
			{
				return await ADatabaseCall.ConfigureAwait(false)
					? SuccessResult.Instance.Sync
					: new FailureResult("ur on cooldown buddy");
			}

			public async ValueTask<IReadOnlyList<TaggedString>> FormatAsync(IContext context)
			{
				var text = await ADatabaseCall.ConfigureAwait(false)
					? "ur good"
					: "ur on cooldown buddy";
				return new[] { new TaggedString(Tag.String, text) };
			}
		}

		[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
		private class EnabledByDefaultAttribute : Attribute, IRuntimeFormattableAttribute
		{
			public bool EnabledByDefault { get; }
			public bool Toggleable { get; set; }

			public EnabledByDefaultAttribute(bool enabledByDefault)
			{
				EnabledByDefault = enabledByDefault;
			}

			public IReadOnlyList<TaggedString> Format(IContext context)
			{
				return new TaggedString[]
				{
					new(Tag.Key, "Enabled by default"),
					new(Tag.Value, EnabledByDefault.ToString()),
					TaggedString.Newline,
					new(Tag.Key, "Toggleable"),
					new(Tag.Value, Toggleable.ToString()),
				};
			}
		}

		[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
		[Summary("The passed in value is less than or equal to 100.")]
		private class LessThanOrEqualTo100Attribute : ParameterPreconditionAttribute
		{
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