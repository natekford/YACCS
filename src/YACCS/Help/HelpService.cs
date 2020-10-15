using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Models;

namespace YACCS.Help
{
	public class HelpService : IHelpService
	{
		private readonly Dictionary<IImmutableCommand, HelpCommand> _Commands
			= new Dictionary<IImmutableCommand, HelpCommand>();
		private readonly IHelpFormatter _Formatter;

		public HelpService(IHelpFormatter formatter)
		{
			_Formatter = formatter;
		}

		public void Add(IImmutableCommand command)
			=> _Commands.Add(command, new HelpCommand(command));

		public Task<string> FormatAsync(IContext context, IImmutableCommand command)
			=> _Formatter.FormatAsync(context, _Commands[command]);

		public void Remove(IImmutableCommand command)
			=> _Commands.Remove(command);
	}
}