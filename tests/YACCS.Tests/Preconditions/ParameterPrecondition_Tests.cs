using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Tests.Preconditions
{
	[TestClass]
	public class ParameterPrecondition_Tests
	{
		[TestMethod]
		public async Task InvalidContext_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var result = await precondition.CheckAsync(default!, default!, new InvalidContext(), 1).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(InvalidContextResult));
		}

		[TestMethod]
		public async Task InvalidValue_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), new object()).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
			Assert.IsInstanceOfType(result, typeof(InvalidParameterResult));
		}

		[TestMethod]
		public async Task MultipleValuesFailure_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var values = new[] { 1, 2, 3, -1, 5 };
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), values).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task MultipleValuesSuccess_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var values = new[] { 1, 2, 3, 4, 5 };
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), values).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task MultipleValuesWithAnInvalidValue_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var values = new object[] { 1, 2, 3, 4, 5, "joe" };
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), values).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task MultipleValuesWithANullValue_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var values = new int?[] { 1, 2, 3, 4, 5, null };
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), values).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task SingleNullValue_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), null!).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task SingleValueFailure_Test()
		{
			IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
			var result = await precondition.CheckAsync(default!, default!, new FakeContext(), -1).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task SingleValueSuccess_Test()
		{
			{
				IParameterPrecondition precondition = new IsNullOrNotNegativeParameterPrecondition();
				var result = await precondition.CheckAsync(default!, default!, new FakeContext(), 1).ConfigureAwait(false);
				Assert.IsTrue(result.IsSuccess);
			}

			{
				IParameterPrecondition<int?> precondition = new IsNullOrNotNegativeParameterPrecondition();
				var result = await precondition.CheckAsync(default!, default!, new FakeContext(), 1).ConfigureAwait(false);
				Assert.IsTrue(result.IsSuccess);
			}
		}

		private class InvalidContext : IContext
		{
			public Guid Id => throw new NotImplementedException();
			public IServiceProvider Services => throw new NotImplementedException();
		}

		private class IsNullOrNotNegativeParameterPrecondition : ParameterPrecondition<FakeContext, int?>
		{
			public override Task<IResult> CheckAsync(
				IImmutableCommand command,
				IImmutableParameter parameter,
				FakeContext context,
				[MaybeNull] int? value)
			{
				if (value is null)
				{
					return SuccessResult.Instance.Task;
				}
				if (value > -1)
				{
					return SuccessResult.Instance.Task;
				}
				return new FailureResult("joe").AsTask();
			}
		}
	}
}