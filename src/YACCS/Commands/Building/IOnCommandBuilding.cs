using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Building
{
	public interface IOnCommandBuilding
	{
		Task ModifyCommandsAsync(IServiceProvider services, List<ReflectionCommand> commands);
	}
}