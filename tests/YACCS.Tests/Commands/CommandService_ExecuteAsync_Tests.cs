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
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._Disabled}"
		).ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandsGroup._DisabledMessage, result.InnerResult.Response);
	}

	[TestMethod]
	public async Task EmptyInput_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(context, "").ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.CommandNotFound, result.InnerResult);
	}

	[TestMethod]
	public async Task EnsureDisposesContext_Test()
	{
		var (commandService, _) = await CreateAsync().ConfigureAwait(false);
		var contextTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var context = new DisposableContext(contextTcs);

		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._Throws}"
		).ConfigureAwait(false);

		await contextTcs.Task.ConfigureAwait(false);

		var commandExecuted = commandService.CommandExecuted.Task;
		Assert.IsTrue(commandExecuted.IsCompleted);
		Assert.IsInstanceOfType<CommandExecutedResult>(commandExecuted.Result);
	}

	[TestMethod]
	public async Task ExecutionDelay_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);

		var sw = new Stopwatch();
		sw.Start();
		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._Delay}"
		).ConfigureAwait(false);
		sw.Stop();
		if (sw.ElapsedMilliseconds >= CommandsGroup.DELAY - 200)
		{
			Assert.Fail("ExecuteAsync did not run in the background.");
		}

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreEqual(CommandsGroup._DelayedMessage, result.InnerResult.Response);
	}

	[TestMethod]
	public async Task ExecutionExceptionAfter_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._ThrowsAfter}"
		).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.EmptySuccess, result.InnerResult);
		Assert.IsNull(result.DuringException);
		Assert.IsNull(result.BeforeExceptions);
		Assert.AreEqual(1, result.AfterExceptions!.Count);
		Assert.IsInstanceOfType<InvalidOperationException>(result.AfterExceptions[0]);
	}

	[TestMethod]
	public async Task ExecutionExceptionBefore_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._ThrowsBefore}"
		).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.EmptySuccess, result.InnerResult);
		Assert.IsNull(result.DuringException);
		Assert.IsNull(result.AfterExceptions);
		Assert.AreEqual(1, result.BeforeExceptions!.Count);
		Assert.IsInstanceOfType<InvalidOperationException>(result.BeforeExceptions[0]);
	}

	[TestMethod]
	public async Task ExecutionExceptionDuring_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(
			context: context,
			input: $"{CommandsGroup._Name} {CommandsGroup._Throws}"
		).ConfigureAwait(false);

		var result = await commandService.CommandExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.ExceptionDuringCommand, result.InnerResult);
		Assert.IsInstanceOfType<InvalidOperationException>(result.DuringException);
		Assert.IsNull(result.BeforeExceptions);
		Assert.IsNull(result.AfterExceptions);
	}

	[TestMethod]
	public async Task Multimatch_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(context, $"{CommandsGroup._Name} 1").ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.MultiMatchHandlingError, result.InnerResult);
	}

	[TestMethod]
	public async Task NoCommandsFound_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(context, "asdf").ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.CommandNotFound, result.InnerResult);
	}

	[TestMethod]
	public async Task QuoteMismatch_Test()
	{
		var (commandService, context) = await CreateAsync().ConfigureAwait(false);
		await commandService.ExecuteAsync(context, "\"an end quote is missing").ConfigureAwait(false);

		var result = await commandService.CommandNotExecuted.Task.ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
		Assert.AreSame(Result.QuoteMismatch, result.InnerResult);
	}

	private static async ValueTask<(FakeCommandService, FakeContext)> CreateAsync()
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

		return (commandService, context);
	}

	private sealed class DisposableContext(TaskCompletionSource tcs)
		: FakeContext, IDisposable
	{
		public void Dispose()
			=> tcs.SetResult();
	}
}