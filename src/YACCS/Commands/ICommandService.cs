using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted;

		event AsyncEventHandler<ExceptionEventArgs<CommandExecutedEventArgs>> CommandExecutedException;

		Task<IResult> ExecuteAsync(IContext context, string input);
	}
}