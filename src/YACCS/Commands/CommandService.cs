using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Parsing;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public class CommandService : CommandServiceBase
	{
		protected IAsyncEvent<CommandExecutedEventArgs> CommandExecutedEvent { get; set; }

		public event Func<CommandExecutedEventArgs, Task> CommandExecuted
		{
			add => CommandExecutedEvent.Add(value);
			remove => CommandExecutedEvent.Remove(value);
		}

		public CommandService(
			ICommandServiceConfig config,
			IArgumentHandler handler,
			IReadOnlyDictionary<Type, ITypeReader> readers)
			: base(config, handler, readers)
		{
			CommandExecutedEvent = new AsyncEvent<CommandExecutedEventArgs>();
		}

		/// <inheritdoc />
		protected override Task OnCommandExecutedAsync(CommandExecutedEventArgs e)
			=> CommandExecutedEvent.InvokeAsync(e);
	}
}