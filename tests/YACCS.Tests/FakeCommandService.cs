using YACCS.Commands;
using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Tests;

public sealed class FakeCommandService(
	CommandServiceConfig config,
	IArgumentHandler handler,
	IReadOnlyDictionary<Type, ITypeReader> readers
) : CommandService(config, handler, readers)
{
	public TaskCompletionSource<CommandExecutedResult> CommandExecuted { get; set; }
		= new(TaskCreationOptions.RunContinuationsAsynchronously);
	public TaskCompletionSource<CommandScore> CommandNotExecuted { get; set; }
		= new(TaskCreationOptions.RunContinuationsAsynchronously);

	protected override async Task CommandExecutedAsync(CommandExecutedResult result)
	{
		CommandExecuted?.SetResult(result);
		await base.CommandExecutedAsync(result).ConfigureAwait(false);
	}

	protected override async Task CommandNotExecutedAsync(CommandScore score)
	{
		CommandNotExecuted?.SetResult(score);
		await base.CommandNotExecutedAsync(score).ConfigureAwait(false);
	}
}