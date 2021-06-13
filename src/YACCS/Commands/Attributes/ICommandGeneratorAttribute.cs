using System.Collections.Generic;

using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	public interface ICommandGeneratorAttribute
	{
		IEnumerable<IImmutableCommand> GenerateCommands(IImmutableCommand original);
	}
}