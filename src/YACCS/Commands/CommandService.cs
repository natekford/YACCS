using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	/// <summary>
	/// A command service which exposes <see cref="CommandExecutedEvent"/> for getting the
	/// real results of commands.
	/// </summary>
	public class CommandService : CommandServiceBase
	{
		/// <summary>
		/// Fires when a command has been executed.
		/// </summary>
		protected IAsyncEvent<CommandExecutedEventArgs> CommandExecutedEvent { get; set; }

		/// <inheritdoc cref="CommandExecutedEvent"/>
		public event Func<CommandExecutedEventArgs, Task> CommandExecuted
		{
			add => CommandExecutedEvent.Add(value);
			remove => CommandExecutedEvent.Remove(value);
		}

		/// <summary>
		/// Creates a new <see cref="CommandService"/>.
		/// </summary>
		/// <param name="config">
		/// <inheritdoc cref="CommandServiceBase.Config" path="/summary"/>
		/// </param>
		/// <param name="handler">
		/// <inheritdoc cref="CommandServiceBase.Handler" path="/summary"/>
		/// </param>
		/// <param name="readers">
		/// <inheritdoc cref="CommandServiceBase.Readers" path="/summary"/>
		/// </param>
		public CommandService(
			ICommandServiceConfig config,
			IArgumentHandler handler,
			IReadOnlyDictionary<Type, ITypeReader> readers)
			: base(config, handler, readers)
		{
			CommandExecutedEvent = new AsyncEvent<CommandExecutedEventArgs>();
		}

		/// <inheritdoc />
		protected override Task CommandExecutedAsync(CommandExecutedEventArgs e)
			=> CommandExecutedEvent.InvokeAsync(e);
	}
}