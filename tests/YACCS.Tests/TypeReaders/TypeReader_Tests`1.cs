using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders;

public abstract class TypeReader_Tests<T>
{
	public virtual IContext Context { get; } = new FakeContext();
	public virtual string Invalid { get; } = "asdf";
	public abstract ITypeReader<T> Reader { get; }

	[TestMethod]
	public async Task Invalid_Test()
	{
		await SetupAsync().ConfigureAwait(false);
		var result = await Reader.ReadAsync(Context, new[] { Invalid }).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
	}

	protected async Task<TResult> AssertFailureAsync<TResult>(string[] input, IContext? context = null)
	{
		await SetupAsync().ConfigureAwait(false);

		context ??= Context;
		var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<TResult>(result.InnerResult);
		return (TResult)result.InnerResult;
	}

	protected async Task<T> AssertSuccessAsync(string[] input, IContext? context = null)
	{
		await SetupAsync().ConfigureAwait(false);

		context ??= Context;
		var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<T>(result.Value);
		return result.Value!;
	}

	protected virtual Task SetupAsync()
		=> Task.CompletedTask;
}