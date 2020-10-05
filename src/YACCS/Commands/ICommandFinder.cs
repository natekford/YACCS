using System.Collections.Generic;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandFinder
	{
		IReadOnlyList<IImmutableCommand> Find(string input);
	}
}