using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Help
{
	public interface IHelpService
	{
		ValueTask<string> FormatAsync(IContext context, IImmutableCommand command);
	}
}