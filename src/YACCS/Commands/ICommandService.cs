using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<ICommand> Commands { get; }

		void Add(ICommand command);

		Task<IReadOnlyList<CommandScore>> GetBestMatchesAsync(
			IContext context,
			IReadOnlyList<string> input,
			IReadOnlyList<CommandScore> candidates);

		Task<CommandScore> ProcessAllPreconditions(
			IContext context,
			IReadOnlyList<string> input,
			CommandScore candidate);

		void Remove(ICommand command);

		IReadOnlyList<CommandScore> TryFind(IReadOnlyList<string> input);
	}
}