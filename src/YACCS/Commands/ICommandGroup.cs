using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandGroup : IContextValidator
	{
		Task AfterExecutionAsync(ICommand command, IContext context);

		Task BeforeExecutionAsync(ICommand command, IContext context);

		Task OnCommandBuildingAsync(IList<IMutableCommand> commands);
	}
}