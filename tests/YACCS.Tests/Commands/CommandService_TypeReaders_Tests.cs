using Microsoft.VisualStudio.TestTools.UnitTesting;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandService_TypeReaders_Tests
{
	[TestMethod]
	public async Task ProcessTypeReaderMultipleButNotAllValues_Test()
	{
		var value = new[] { 1, 2, 3, 4 };
		var input = value.Select(x => x.ToString()).Append("joeba").Append("trash").ToArray();
		var result = await RunAsync<int[]>(4, 0, input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);

		var cast = (IReadOnlyList<int>)result.Value!;
		for (var i = 0; i < value.Length; ++i)
		{
			Assert.AreEqual(value[i], cast[i]);
		}
	}

	[TestMethod]
	public async Task ProcessTypeReaderMultipleValuesAllValues_Test()
	{
		var value = new[] { 1, 2, 3, 4 };
		var input = value.Select(x => x.ToString()).ToArray();
		var result = await RunAsync<int[]>(4, 0, input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);

		var cast = (IReadOnlyList<int>)result.Value!;
		for (var i = 0; i < value.Length; ++i)
		{
			Assert.AreEqual(value[i], cast[i]);
		}
	}

	[TestMethod]
	public async Task ProcessTypeReaderMultipleValuesLongerThanArgs_Test()
	{
		var value = new[] { 1, 2, 3, 4 };
		var input = value.Select(x => x.ToString()).ToArray();
		var result = await RunAsync<int[]>(null, 0, input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);

		var cast = (IReadOnlyList<int>)result.Value!;
		for (var i = 0; i < value.Length; ++i)
		{
			Assert.AreEqual(value[i], cast[i]);
		}
	}

	[TestMethod]
	public async Task ProcessTypeReaderNotRegistered_Test()
	{
		await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
		{
			_ = await RunAsync<DBNull>(1, 0, new[] { "joeba" }).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ProcessTypeReaderOverridden_Test()
	{
		var result = await RunAsync<char>(1, 0, new[] { "joeba" }, new CoolCharTypeReader()).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ProcessTypeReadersCharFailure_Test()
	{
		var result = await RunAsync<char>(1, 0, new[] { "joeba" }).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ProcessTypeReaderSingleValue_Test()
	{
		const int VALUE = 2;
		var result = await RunAsync<int>(1, 0, new[] { VALUE.ToString() }).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.Value, VALUE.GetType());
		Assert.AreEqual(VALUE, result.Value);
	}

	[TestMethod]
	public async Task ProcessTypeReaderSingleValueWhenMultipleExist_Test()
	{
		const int VALUE = 2;
		var result = await RunAsync<int>(1, 0, new[] { VALUE.ToString(), "joeba", "trash" }).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.Value, VALUE.GetType());
		Assert.AreEqual(VALUE, result.Value);
	}

	[TestMethod]
	public async Task ProcessTypeReadersOneInvalidValue_Test()
	{
		var result = await RunAsync<char[]>(4, 0, new[] { "a", "b", "cee", "d" }).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ProcessTypeReadersString_Test()
	{
		const string VALUE = "joeba";
		var result = await RunAsync<string>(1, 0, new[] { VALUE }).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.Value, VALUE.GetType());
		Assert.AreEqual(VALUE, result.Value);
	}

	[TestMethod]
	public async Task ProcessTypeReaderZeroLength_Test()
	{
		var result = await RunAsync<IContext>(0, 0, new[] { "doesn't matter" }).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.Value, typeof(IContext));
	}

	private static ITask<ITypeReaderResult> RunAsync<T>(
		int? length,
		int startIndex,
		ReadOnlyMemory<string> input,
		ITypeReader? reader = null)
	{
		var parameter = new Parameter(typeof(T), "", null)
		{
			Attributes =
				[
					new LengthAttribute(length),
				],
			TypeReader = reader,
		}.ToImmutable();
		var context = new FakeContext();
		var commandService = context.Get<FakeCommandService>();
		return commandService.ProcessTypeReadersAsync(context, parameter, input, startIndex);
	}

	private class CoolCharTypeReader : TypeReader<char>
	{
		public override ITask<ITypeReaderResult<char>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
			=> Success('z').AsITask();
	}
}