using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands.Building
{
	/// <summary>
	/// Defines a method for modifying commands after the command group has been created.
	/// </summary>
	public interface IOnCommandBuilding
	{
		/// <summary>
		/// Modifies commands after they have been created.
		/// </summary>
		/// <param name="services">The services to use for dependency injection.</param>
		/// <param name="commands">The list of newly created commands.</param>
		/// <returns></returns>
		Task ModifyCommandsAsync(IServiceProvider services, List<ReflectionCommand> commands);
	}
}