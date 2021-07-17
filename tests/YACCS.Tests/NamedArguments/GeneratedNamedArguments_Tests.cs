using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.NamedArguments;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.NamedArguments
{
	[TestClass]
	public class GeneratedNamedArguments_Tests
	{
		private const double DOUBLE = 2.2;
		private const int INT = 1;
		private const string STRING = "three";

		[TestMethod]
		public async Task Class_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: {INT}" +
				$" {CommandsGroup.S}: {STRING}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);

			Assert.AreEqual(INT, setMe.IntValue);
			Assert.AreEqual(DOUBLE, setMe.DoubleValue);
			Assert.AreEqual(STRING, setMe.StringValue);
		}

		[TestMethod]
		public async Task ClassInvalidValue_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: -1" +
				$" {CommandsGroup.S}: {STRING}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(InvalidParameterResult));
		}

		[TestMethod]
		public async Task ClassUnparsableValue_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: asdf" +
				$" {CommandsGroup.S}: {STRING}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(ParseFailedResult<int>));
		}

		[TestMethod]
		public async Task NonExistentName_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			const string FAKE_NAME = "joeba";
			var input = nameof(CommandsGroup.Test2) +
				$" {CommandsGroup.I}: {INT}" +
				$" {FAKE_NAME}: {STRING}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(NamedArgNonExistentResult));
			Assert.AreEqual(FAKE_NAME, ((NamedArgNonExistentResult)result.InnerResult).Name);
		}

		[TestMethod]
		public async Task ParameterAllArgs_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

			var input = nameof(CommandsGroup.Test) +
				$" {CommandsGroup.I}: {INT}" +
				$" {CommandsGroup.S}: {STRING}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);

			await tcs.Task.ConfigureAwait(false);

			Assert.AreEqual(INT, setMe.IntValue);
			Assert.AreEqual(DOUBLE, setMe.DoubleValue);
			Assert.AreEqual(STRING, setMe.StringValue);
		}

		[TestMethod]
		public async Task ParameterMissingOneArg_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			var tcs = new TaskCompletionSource();
			commandService.CommandExecuted += (e) =>
			{
				tcs.SetResult();
				return Task.CompletedTask;
			};

			var input = nameof(CommandsGroup.Test) +
				$" {CommandsGroup.I}: {INT}" +
				$" {CommandsGroup.D}: {DOUBLE}";
			var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);

			await tcs.Task.ConfigureAwait(false);

			Assert.AreEqual(INT, setMe.IntValue);
			Assert.AreEqual(DOUBLE, setMe.DoubleValue);
			Assert.AreEqual(CommandsGroup.S_DEFAULT, setMe.StringValue);
		}

		[TestMethod]
		public async Task ParameterWithoutDefaultValue_Test()
		{
			var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

			const string INPUT = nameof(CommandsGroup.Test3);
			var result = await commandService.ExecuteAsync(context, INPUT).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(NamedArgMissingValueResult));
		}

		private async ValueTask<(CommandService, SetMe, FakeContext)> CreateAsync()
		{
			var setMe = new SetMe();
			var context = new FakeContext()
			{
				Services = Utils.CreateServiceCollection().AddSingleton(setMe).BuildServiceProvider(),
			};

			var commandService = context.Get<CommandService>();
			var commands = typeof(CommandsGroup).GetAllCommandsAsync(context.Services);
			await commandService.AddRangeAsync(commands).ConfigureAwait(false);

			return (commandService, setMe, context);
		}

		private class CommandsGroup : CommandGroup<IContext>
		{
			public const string D = "val_d";
			public const string I = "val_i";
			public const string S = "val_s";
			public const string S_DEFAULT = "73 xd";

			public SetMe SetMe { get; set; } = null!;

			[Command(nameof(Test))]
			[GenerateNamedArguments]
			public async Task<IResult> Test(
				[Name(D)]
				[Test]
				double d,
				[Name(I)]
				int i,
				[Name(S)]
				string s = S_DEFAULT)
			{
				await Task.Delay(50).ConfigureAwait(false);

				SetMe.DoubleValue = d;
				SetMe.IntValue = i;
				SetMe.StringValue = s;

				return SuccessResult.Instance;
			}

			[Command(nameof(Test2))]
			public void Test2(NamedArgs @class)
			{
				SetMe.DoubleValue = @class.D;
				SetMe.IntValue = @class.I;
				SetMe.StringValue = @class.S;
			}

			[Command(nameof(Test3))]
			[GenerateNamedArguments]
			public void Test3([Name(D)] double d)
				=> SetMe.DoubleValue = d;
		}

		[GenerateNamedArguments]
		private class NamedArgs
		{
			[Name(CommandsGroup.D)]
			public double D { get; set; }
			[Name(CommandsGroup.I)]
			[NotNegative]
			public int I { get; set; }
			[Name(CommandsGroup.S)]
			public string S { get; set; } = "";
		}

		private class NotNegative : ParameterPreconditionAttribute
		{
			public ValueTask<IResult> CheckAsync(
				CommandMeta meta,
				IContext context,
				int value)
			{
				if (value > -1)
				{
					return new(SuccessResult.Instance);
				}
				return new(InvalidParameterResult.Instance);
			}

			protected override ValueTask<IResult> CheckAsync(
				CommandMeta meta,
				IContext context,
				object? value)
				=> this.CheckAsync<IContext, int>(meta, context, value, CheckAsync);
		}

		private class SetMe
		{
			public double DoubleValue { get; set; }
			public int IntValue { get; set; }
			public string StringValue { get; set; } = null!;
		}

		[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
		private class TestAttribute : Attribute, IParameterModifierAttribute
		{
			public void ModifyParameter(IParameter parameter)
			{
				for (var i = 0; i < 10; ++i)
				{
					parameter.Attributes.Add("test");
				}
			}
		}
	}
}