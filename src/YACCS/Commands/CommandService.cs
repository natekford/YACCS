using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Commands;

/// <summary>
/// A command service which exposes <see cref="CommandExecutedEvent"/> for getting the
/// real results of commands.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandService"/>.
/// </remarks>
/// <param name="config">
/// <inheritdoc cref="CommandServiceBase.Config" path="/summary"/>
/// </param>
/// <param name="handler">
/// <inheritdoc cref="CommandServiceBase.Handler" path="/summary"/>
/// </param>
/// <param name="readers">
/// <inheritdoc cref="CommandServiceBase.Readers" path="/summary"/>
/// </param>
public class CommandService(
	ICommandServiceConfig config,
	IArgumentHandler handler,
	IReadOnlyDictionary<Type, ITypeReader> readers)
	: CommandServiceBase(config, handler, readers)
{
	/// <summary>
	/// Fires when a command has been executed.
	/// </summary>
	protected IAsyncEvent<CommandExecutedEventArgs> CommandExecutedEvent { get; set; } = new AsyncEvent<CommandExecutedEventArgs>();

	/// <inheritdoc cref="CommandExecutedEvent"/>
	public event Func<CommandExecutedEventArgs, Task> CommandExecuted
	{
		add => CommandExecutedEvent.Add(value);
		remove => CommandExecutedEvent.Remove(value);
	}

	/// <inheritdoc />
	protected override Task CommandExecutedAsync(CommandExecutedEventArgs e)
		=> CommandExecutedEvent.InvokeAsync(e);
}