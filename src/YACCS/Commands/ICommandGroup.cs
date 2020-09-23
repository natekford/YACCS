using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandGroup : IContextValidator
	{
		Task AfterExecutionAsync(IImmutableCommand command, IContext context);

		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);

		Task OnCommandBuildingAsync(IList<ICommand> commands);
	}
}