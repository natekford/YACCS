using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands;

[TestClass]
public sealed class CommandService_Tests
{
	[TestMethod]
	public async Task OptionalEnumNoValue_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		const string INPUT = nameof(OptionalValueType.EnumDoThing);
		await commandService.ExecuteAsync(context, INPUT).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(BindingFlags.Instance.ToString(), result.InnerResult.Response);
	}

	[TestMethod]
	public async Task OptionalEnumYesValue_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		var expected = BindingFlags.GetField.ToString();
		var input = $"{nameof(OptionalValueType.EnumDoThing)} {expected}";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(expected, result.InnerResult.Response);
	}

	[TestMethod]
	public async Task OptionalValueTypeNoValue_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		const string INPUT = nameof(OptionalValueType.DoThing);
		await commandService.ExecuteAsync(context, INPUT).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsEmpty(result.InnerResult.Response);
	}

	[TestMethod]
	public async Task OptionalValueTypeYesValue_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		const string EXPECTED = "dog";
		var input = $"{nameof(OptionalValueType.DoThing)} {EXPECTED}";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(EXPECTED, result.InnerResult.Response);
	}

	[TestMethod]
	public async Task PrioritizeCommandThatFailsParameterPrecondition_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		var input = $"{nameof(BanCommands.Ban)} 1234";
		await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(BanCommands.AlwaysFails.RESPONSE, result.InnerResult.Response);
	}

	private static async ValueTask<(FakeCommandService, FakeContext)> CreateAsync()
	{
		var context = new FakeContext();

		var readers = context.Get<TypeReaderRegistry>();
		readers.RegisterStruct(new TryParseTypeReader<ValueType>((string s, out ValueType result) =>
		{
			result = new(s);
			return true;
		}));

		var commandService = context.Get<FakeCommandService>();
		foreach (var type in new[] { typeof(BanCommands), typeof(OptionalValueType) })
		{
			var commands = type.GetDirectCommandsAsync(context.Services);
			await commandService.AddRangeAsync(commands).ConfigureAwait(false);
		}

		return (commandService, context);
	}

	private readonly struct ValueType
	{
		public const string DEFAULT = "asdf";

		public string String { get; }

		public ValueType()
		{
			String = DEFAULT;
		}

		public ValueType(string s)
		{
			String = s;
		}
	}

	private class BanCommands : CommandGroup<FakeContext>
	{
		public const string BAN1 = "1";
		public const string BAN2 = "2";

		[Command(nameof(Ban))]
		[YACCS.Commands.Attributes.Priority(1)]
		public string Ban([AlwaysFails] string id)
			=> BAN1;

		[Command(nameof(Ban))]
		[YACCS.Commands.Attributes.Priority(0)]
		public string Ban(ulong id)
			=> BAN2;

		public sealed class AlwaysFails : ParameterPrecondition<IContext, string>
		{
			public const string RESPONSE = "nuh uh";

			protected override ValueTask<IResult> CheckNotNullAsync(CommandMeta meta, IContext context, string value)
				=> new(Result.Failure(RESPONSE));
		}
	}

	private class OptionalValueType : CommandGroup<FakeContext>
	{
		[Command(nameof(DoThing))]
		public string DoThing(ValueType value = default)
			=> value.String;

		[Command(nameof(EnumDoThing))]
		public string EnumDoThing(BindingFlags value = BindingFlags.Instance)
			=> value.ToString();
	}
}