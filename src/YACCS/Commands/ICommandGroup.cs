using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandGroup
	{
		ValueTask AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result);

		ValueTask BeforeExecutionAsync(IImmutableCommand command, IContext context);

		ValueTask OnCommandBuildingAsync(IList<ICommand> commands);
	}
}