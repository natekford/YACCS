using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Interactivity;
using YACCS.Interactivity.Input;
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
		var commands = typeof(OptionalValueType).GetDirectCommandsAsync(context.Services);
		await commandService.AddRangeAsync(commands).ConfigureAwait(false);

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