using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<IImmutableCommand> Commands { get; }

		event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted;

		event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException;

		void Add(IImmutableCommand command);

		Task<IResult> ExecuteAsync(IContext context, string input);

		void Remove(IImmutableCommand command);
	}
}