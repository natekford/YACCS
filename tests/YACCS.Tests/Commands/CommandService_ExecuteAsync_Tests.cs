using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Tests.Commands;

[TestClass]
public class CommandService_ExecuteAsync_Tests
{
	[TestMethod]
	public async Task BestMatchIsDisabled_Test()
	{
		var (commandService, context, _) = await CreateAsync().ConfigureAwait(false);
		const string input = $"{CommandsGroup._Name} {CommandsGroup._Disabled}";
		var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandsGroup._DisabledMessage, result.InnerResult.Response);
	}

	[TestMethod]
	public async Task EmptyInput_Test()
	{
		var (commandService, context, _) = await CreateAsync().ConfigureAwait(false);
		var result = await commandService.ExecuteAsync(context, "").ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(CachedResults.CommandNotFound, result.InnerResult);
	}

	[TestMethod]
	public async Task EnsureDisposesContext_Test()
	{
		var (commandService, _, tcs) = await CreateAsync().ConfigureAwait(false);
		var contextTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var context = new DisposableContext(contextTcs);

		const string input = $"{CommandsGroup._Name} {CommandsGroup._Throws}";
		var syncResult = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(syncResult.InnerResult.IsSuccess);

		await contextTcs.Task.ConfigureAwait(false);
		Assert.IsTrue(tcs.Task.IsCompleted);
		Assert.IsInstanceOfType(tcs.Task.Result, typeof(CommandExecutedEventArgs));
	}

	[TestMethod]
	public async Task ExecutionDelay_Test()
	{
		var (commandService, context, tcs) = await CreateAsync().ConfigureAwait(false);

		const string input = $"{CommandsGroup._Name} {CommandsGroup._Delay}";
		var sw = new Stopwatch();
		sw.Start();
		var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		sw.Stop();
		Assert.IsTrue(result.InnerResult.IsSuccess);
		if (sw.ElapsedMilliseconds >= CommandsGroup.DELAY - 200)
		{
			Assert.Fail("ExecuteAsync did not run in the background.");
		}

		var eArgs = await tcs.Task.ConfigureAwait(false);
		var eResult = eArgs.Result;
		Assert.IsFalse(eResult.IsSuccess);
		Assert.AreEqual(CommandsGroup._DelayedMessage, eResult.Response);
	}

	[TestMethod]
	public async Task ExecutionExceptionAfter_Test()
	{
		var (commandService, context, tcs) = await CreateAsync().ConfigureAwait(false);

		const string input = $"{CommandsGroup._Name} {CommandsGroup._ThrowsAfter}";
		var syncResult = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(syncResult.InnerResult.IsSuccess);

		var asyncResult = await tcs.Task.ConfigureAwait(false);
		Assert.IsTrue(asyncResult.Result.IsSuccess);
		Assert.AreSame(CachedResults.Success, asyncResult.Result);
		Assert.IsNull(asyncResult.DuringException);
		Assert.IsNull(asyncResult.BeforeExceptions);
		Assert.AreEqual(1, asyncResult.AfterExceptions!.Count);
		Assert.IsInstanceOfType(asyncResult.AfterExceptions[0], typeof(InvalidOperationException));
	}

	[TestMethod]
	public async Task ExecutionExceptionBefore_Test()
	{
		var (commandService, context, tcs) = await CreateAsync().ConfigureAwait(false);

		const string input = $"{CommandsGroup._Name} {CommandsGroup._ThrowsBefore}";
		var syncResult = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(syncResult.InnerResult.IsSuccess);

		var asyncResult = await tcs.Task.ConfigureAwait(false);
		Assert.IsTrue(asyncResult.Result.IsSuccess);
		Assert.AreSame(CachedResults.Success, asyncResult.Result);
		Assert.IsNull(asyncResult.DuringException);
		Assert.IsNull(asyncResult.AfterExceptions);
		Assert.AreEqual(1, asyncResult.BeforeExceptions!.Count);
		Assert.IsInstanceOfType(asyncResult.BeforeExceptions[0], typeof(InvalidOperationException));
	}

	[TestMethod]
	public async Task ExecutionExceptionDuring_Test()
	{
		var (commandService, context, tcs) = await CreateAsync().ConfigureAwait(false);

		const string input = $"{CommandsGroup._Name} {CommandsGroup._Throws}";
		var syncResult = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);
		Assert.IsTrue(syncResult.InnerResult.IsSuccess);

		var asyncResult = await tcs.Task.ConfigureAwait(false);
		Assert.IsFalse(asyncResult.Result.IsSuccess);
		Assert.AreSame(CachedResults.ExceptionDuringCommand, asyncResult.Result);
		Assert.IsInstanceOfType(asyncResult.DuringException, typeof(InvalidOperationException));
		Assert.IsNull(asyncResult.BeforeExceptions);
		Assert.IsNull(asyncResult.AfterExceptions);
	}

	[TestMethod]
	public async Task Multimatch_Test()
	{
		var (commandService, context, _) = await CreateAsync().ConfigureAwait(false);
		const string input = $"{CommandsGroup._Name} 1";
		var result = await commandService.ExecuteAsync(context, input).ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(CachedResults.MultiMatchHandlingError, result.InnerResult);
	}

	[TestMethod]
	public async Task NoCommandsFound_Test()
	{
		var (commandService, context, _) = await CreateAsync().ConfigureAwait(false);
		var result = await commandService.ExecuteAsync(context, "asdf").ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(CachedResults.CommandNotFound, result.InnerResult);
	}

	[TestMethod]
	public async Task QuoteMismatch_Test()
	{
		var (commandService, context, _) = await CreateAsync().ConfigureAwait(false);
		var result = await commandService.ExecuteAsync(context, "\"an end quote is missing").ConfigureAwait(false);

		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(CachedResults.QuoteMismatch, result.InnerResult);
	}

	private static async ValueTask<(FakeCommandService, FakeContext, TaskCompletionSource<CommandExecutedEventArgs>)> CreateAsync()
	{
		var context = new FakeContext
		{
			Services = Utils.CreateServices(CommandServiceConfig.Default with
			{
				MultiMatchHandling = MultiMatchHandling.Error,
			}),
		};

		var commandService = context.Get<FakeCommandService>();
		var commands = typeof(CommandsGroup).GetDirectCommandsAsync(context.Services);
		await commandService.AddRangeAsync(commands).ConfigureAwait(false);

		var tcs = new TaskCompletionSource<CommandExecutedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
		commandService.CommandExecuted = e =>
		{
			tcs.SetResult(e);
			return Task.CompletedTask;
		};

		return (commandService, context, tcs);
	}

	private sealed class DisposableContext(TaskCompletionSource tcs)
		: FakeContext, IDisposable
	{
		public void Dispose()
			=> tcs.SetResult();
	}
}