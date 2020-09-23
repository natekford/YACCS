using System.ComponentModel;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public class CommandExecutedEventArgs : HandledEventArgs
	{
		public ICommand Command { get; }
		public IContext Context { get; }
		public IResult Result { get; }

		public CommandExecutedEventArgs(ICommand command, IContext context, IResult result)
		{
			Command = command;
			Context = context;
			Result = result;
		}
	}
}