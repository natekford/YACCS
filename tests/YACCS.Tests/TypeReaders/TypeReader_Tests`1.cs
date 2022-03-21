using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

public abstract class TypeReader_Tests<T>
{
	public virtual IContext Context { get; } = new FakeContext();
	public virtual Type ExpectedInvalidResultType { get; } = typeof(ParseFailed);
	public virtual string Invalid { get; } = "asdf";
	public abstract ITypeReader<T> Reader { get; }

	[TestMethod]
	public async Task Invalid_Test()
	{
		await SetupAsync().ConfigureAwait(false);
		var result = await Reader.ReadAsync(Context, new[] { Invalid }).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.InnerResult, ExpectedInvalidResultType);
	}

	protected Task<TResult> AssertFailureAsync<TResult>(string input, IContext? context = null)
		=> AssertFailureAsync<TResult>(new[] { input }, context);

	protected async Task<TResult> AssertFailureAsync<TResult>(string[] input, IContext? context = null)
	{
		await SetupAsync().ConfigureAwait(false);

		context ??= Context;
		var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.InnerResult, typeof(TResult));
		return (TResult)result.InnerResult;
	}

	protected Task<T> AssertSuccessAsync(string input, IContext? context = null)
		=> AssertSuccessAsync(new[] { input }, context);

	protected async Task<T> AssertSuccessAsync(string[] input, IContext? context = null)
	{
		await SetupAsync().ConfigureAwait(false);

		context ??= Context;
		var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType(result.Value, typeof(T));
		return result.Value!;
	}

	protected virtual Task SetupAsync()
		=> Task.CompletedTask;
}