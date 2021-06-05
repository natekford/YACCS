using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<IImmutableCommand> Commands { get; }

		Task<ICommandResult> ExecuteAsync(IContext context, string input);

		IReadOnlyList<IImmutableCommand> Find(string input);
	}
}