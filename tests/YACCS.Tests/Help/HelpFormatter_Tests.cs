using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class HelpFormatter_Tests
	{
		[TestMethod]
		public async Task Standard_Test()
		{
			var commands = await typeof(CommandGroup).GetDirectCommandsAsync().ConfigureAwait(false);
			var help = new HelpCommand(commands.Single());
			var formatter = new HelpFormatter(new TypeNameRegistry());
			var context = new FakeContext();

			var text = await formatter.FormatAsync(context, help).ConfigureAwait(false);
			Debug.WriteLine(text);
		}

		private class CommandGroup : CommandGroup<IContext>
		{
			[Command(nameof(Method))]
			[EnabledByDefault(true, Toggleable = false)]
			[Summary("Throws an exception if the passed in value is greater than 100.")]
			public void Method(
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
		private class EnabledByDefaultAttribute : Attribute, IRuntimeFormattableAttribute
		{
			public bool EnabledByDefault { get; }
			public bool Toggleable { get; set; }

			public EnabledByDefaultAttribute(bool enabledByDefault)
			{
				EnabledByDefault = enabledByDefault;
			}

			public ValueTask<string> FormatAsync(IContext context)
				=> new ValueTask<string>($"Enabled by default = {EnabledByDefault}, toggleable = {Toggleable}");
		}

		[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
		[Summary("The passed in value is less than or equal to 100.")]
		private class LessThanOrEqualTo100Attribute : ParameterPreconditionAttribute
		{
			public override Task<IResult> CheckAsync(
				ParameterInfo parameter,
				IContext context,
				object? value)
			{
				return this.CheckAsync<IContext, int>(parameter, context, value, (p, c, v) =>
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