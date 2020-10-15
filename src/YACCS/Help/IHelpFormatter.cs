using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Help.Models;

namespace YACCS.Help
{
	public interface IHelpFormatter
	{
		Task<string> FormatAsync(IContext context, HelpCommand command);
	}
}