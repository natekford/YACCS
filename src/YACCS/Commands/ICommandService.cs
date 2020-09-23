using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<ICommand> Commands { get; }

		event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted;

		event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException;

		void Add(ICommand command);

		Task<IReadOnlyList<CommandScore>> GetBestMatchesAsync(
			IContext context,
			IReadOnlyList<string> input,
			IReadOnlyList<CommandScore> candidates);

		IReadOnlyList<CommandScore> GetCommands(string input);

		IReadOnlyList<CommandScore> GetCommands(IReadOnlyList<string> input);

		Task<CommandScore> ProcessAllPreconditions(
			IContext context,
			IReadOnlyList<string> input,
			CommandScore candidate);

		void Remove(ICommand command);
	}
}