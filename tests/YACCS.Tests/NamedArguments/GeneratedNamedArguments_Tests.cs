using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.NamedArguments;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.NamedArguments;

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
		await ExecuteSuccessfulCommandAsync(commandService, context, input).ConfigureAwait(false);

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
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.InvalidParameter, result.InnerResult);
	}

	[TestMethod]
	public async Task ClassUnparsableValue_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = nameof(CommandsGroup.Test2) +
			$" {CommandsGroup.I}: asdf" +
			$" {CommandsGroup.S}: {STRING}" +
			$" {CommandsGroup.D}: {DOUBLE}";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<ParseFailed>(result.InnerResult);
	}

	[TestMethod]
	public async Task InvokingCommandNormally_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = $"{nameof(CommandsGroup.Test)} {DOUBLE} {INT} {STRING}";
		await ExecuteSuccessfulCommandAsync(commandService, context, input).ConfigureAwait(false);

		Assert.AreEqual(INT, setMe.IntValue);
		Assert.AreEqual(DOUBLE, setMe.DoubleValue);
		Assert.AreEqual(STRING, setMe.StringValue);
	}

	[TestMethod]
	public async Task InvokingCommandNormallyMissingArg_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = $"{nameof(CommandsGroup.Test)} {DOUBLE}";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.NotEnoughArgs, result.InnerResult);
	}

	[TestMethod]
	public async Task InvokingCommandNormallyMissingOptionalArg_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = $"{nameof(CommandsGroup.Test)} {DOUBLE} {INT}";
		await ExecuteSuccessfulCommandAsync(commandService, context, input).ConfigureAwait(false);

		Assert.AreEqual(INT, setMe.IntValue);
		Assert.AreEqual(DOUBLE, setMe.DoubleValue);
		Assert.AreEqual(CommandsGroup.S_DEFAULT, setMe.StringValue);
	}

	[TestMethod]
	public async Task InvokingCommandNormallyTooManyArgs_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = $"{nameof(CommandsGroup.Test)} {DOUBLE} {INT} {STRING} a b c d";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.TooManyArgs, result.InnerResult);
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
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<NamedArgNonExistent>(result.InnerResult);
		Assert.AreEqual(FAKE_NAME, ((LocalizedResult<string>)result.InnerResult).Value);
	}

	[TestMethod]
	public async Task ParameterAllArgs_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = nameof(CommandsGroup.Test) +
			$" {CommandsGroup.I}: {INT}" +
			$" {CommandsGroup.S}: {STRING}" +
			$" {CommandsGroup.D}: {DOUBLE}";
		await ExecuteSuccessfulCommandAsync(commandService, context, input).ConfigureAwait(false);

		Assert.AreEqual(INT, setMe.IntValue);
		Assert.AreEqual(DOUBLE, setMe.DoubleValue);
		Assert.AreEqual(STRING, setMe.StringValue);
	}

	[TestMethod]
	public async Task ParameterMissingAll_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		const string INPUT = nameof(CommandsGroup.Test3);
		await commandService.ExecuteAsync(context, INPUT).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<NotEnoughArgs>(result.InnerResult);
	}

	[TestMethod]
	public async Task ParameterMissingOneArg_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = nameof(CommandsGroup.Test) +
			$" {CommandsGroup.I}: {INT}" +
			$" {CommandsGroup.S}: {STRING}";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<NamedArgMissingValue>(result.InnerResult);
	}

	[TestMethod]
	public async Task ParameterMissingOptionalArg_Test()
	{
		var (commandService, setMe, context) = await CreateAsync().ConfigureAwait(false);

		var input = nameof(CommandsGroup.Test) +
			$" {CommandsGroup.I}: {INT}" +
			$" {CommandsGroup.D}: {DOUBLE}";
		await ExecuteSuccessfulCommandAsync(commandService, context, input).ConfigureAwait(false);

		Assert.AreEqual(INT, setMe.IntValue);
		Assert.AreEqual(DOUBLE, setMe.DoubleValue);
		Assert.AreEqual(CommandsGroup.S_DEFAULT, setMe.StringValue);
	}

	private static async ValueTask<(FakeCommandService, SetMe, FakeContext)> CreateAsync()
	{
		var setMe = new SetMe();
		var context = new FakeContext()
		{
			Services = Utils.CreateServiceCollection().AddSingleton(setMe).BuildServiceProvider(),
		};

		var commandService = context.Get<FakeCommandService>();
		var commands = typeof(CommandsGroup).GetAllCommandsAsync(context.Services);
		await commandService.AddRangeAsync(commands).ConfigureAwait(false);

		return (commandService, setMe, context);
	}

	private static async Task ExecuteSuccessfulCommandAsync(
		FakeCommandService commandService,
		IContext context,
		string input)
	{
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
	}

	private class CommandsGroup : CommandGroup<IContext>
	{
		public const string D = "val_d";
		public const string I = "val_i";
		public const string S = "val_s";
		public const string S_DEFAULT = "73 xd";

		[InjectService]
		public SetMe SetMe { get; set; } = null!;

		[Command(nameof(Test))]
		[GenerateNamedArguments]
		public async Task<IResult> Test(
			[Name(D)]
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

			return Result.EmptySuccess;
		}

		[Command(nameof(Test2))]
		public void Test2(NamedArgs @class)
		{
			SetMe.DoubleValue = @class.D;
			SetMe.IntValue = @class.I;
			SetMe.StringValue = @class.S;
		}

		[Command(nameof(Test3))]
		[GenerateNamedArguments(PriorityDifference = 0)]
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

	private class NotNegative : ParameterPrecondition<IContext, int>
	{
		public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();

		protected override ValueTask<IResult> CheckNotNullAsync(
			CommandMeta meta,
			IContext context,
			int value)
		{
			if (value > -1)
			{
				return new(Result.EmptySuccess);
			}
			return new(Result.InvalidParameter);
		}
	}

	private class SetMe
	{
		public double DoubleValue { get; set; }
		public int IntValue { get; set; }
		public string StringValue { get; set; } = null!;
	}
}