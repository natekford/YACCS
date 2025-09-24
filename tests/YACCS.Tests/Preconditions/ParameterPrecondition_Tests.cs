using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Preconditions;

[TestClass]
public class ParameterPrecondition_Tests
{
	private readonly IParameterPrecondition _Precondition = new IsNullOrNotNegative();

	[TestMethod]
	public async Task InvalidContext_Test()
	{
		var result = await _Precondition.CheckAsync(default, new OtherContext(), 1).ConfigureAwait(false);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreSame(Result.InvalidContext, result);
	}

	[TestMethod]
	public async Task InvalidValue_Test()
	{
		var result = await _Precondition.CheckAsync(default, new FakeContext(), new object()).ConfigureAwait(false);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreSame(Result.InvalidParameter, result);
	}

	[TestMethod]
	public async Task MultipleValuesFailure_Test()
	{
		var values = new[] { 1, 2, 3, -1, 5 };
		var result = await _Precondition.CheckAsync(default, new FakeContext(), values).ConfigureAwait(false);

		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task MultipleValuesSuccess_Test()
	{
		var values = new[] { 1, 2, 3, 4, 5 };
		var result = await _Precondition.CheckAsync(default, new FakeContext(), values).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task MultipleValuesWithAnInvalidValue_Test()
	{
		var values = new object[] { 1, 2, 3, 4, 5, "joe" };
		var result = await _Precondition.CheckAsync(default, new FakeContext(), values).ConfigureAwait(false);

		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task MultipleValuesWithANullValue_Test()
	{
		var values = new int?[] { 1, 2, 3, 4, 5, null };
		var result = await _Precondition.CheckAsync(default, new FakeContext(), values).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task SingleNullValue_Test()
	{
		var result = await _Precondition.CheckAsync(default, new FakeContext(), null).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task SingleValueFailure_Test()
	{
		var result = await _Precondition.CheckAsync(default, new FakeContext(), -1).ConfigureAwait(false);

		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task SingleValueGenericValueSuccess_Test()
	{
		var precondition = (IParameterPrecondition<FakeContext, int?>)_Precondition;
		var result = await precondition.CheckAsync(default, new FakeContext(), 1).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task SingleValueSuccess_Test()
	{
		var result = await _Precondition.CheckAsync(default, new FakeContext(), 1).ConfigureAwait(false);

		Assert.IsTrue(result.IsSuccess);
	}

	private class IsNullOrNotNegative : SummarizableParameterPrecondition<FakeContext, int?>
	{
		public override ValueTask<string> GetSummaryAsync(FakeContext context, IFormatProvider? formatProvider = null)
			=> throw new NotImplementedException();

		protected override ValueTask<IResult> CheckNotNullAsync(
			CommandMeta meta,
			FakeContext context,
			int? value)
		{
			if (value > -1)
			{
				return new(Result.EmptySuccess);
			}
			return new(Result.Failure("joe"));
		}

		protected override ValueTask<IResult> CheckNullAsync(
			CommandMeta meta,
			FakeContext context)
			=> new(Result.EmptySuccess);
	}
}