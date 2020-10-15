using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Help.Models;

namespace YACCS.Help
{
	public interface IHelpFormatter
	{
		ValueTask<string> FormatAsync(IContext context, IHelpCommand command);
	}
}