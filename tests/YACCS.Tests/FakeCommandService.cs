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
	public Func<CommandExecutedArgs, Task>? CommandExecuted { get; set; }

	protected override async Task CommandExecutedAsync(CommandExecutedArgs e)
	{
		if (CommandExecuted != null)
		{
			await CommandExecuted.Invoke(e).ConfigureAwait(false);
		}
		await base.CommandExecutedAsync(e).ConfigureAwait(false);
	}
}