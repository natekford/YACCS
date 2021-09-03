using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Building
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public abstract class OnCommandBuildingAttribute : Attribute
	{
		public abstract Task ModifyCommands(IServiceProvider services, List<ReflectionCommand> commands);
	}
}