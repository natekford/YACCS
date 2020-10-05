using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted;

		event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException;

		Task<IResult> ExecuteAsync(IContext context, string input);

		IReadOnlyList<IImmutableCommand> Find(string input);
	}
}