using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandGroup
	{
		Task AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result);

		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);

		Task OnCommandBuildingAsync(IServiceProvider services, IList<ICommand> commands);
	}
}