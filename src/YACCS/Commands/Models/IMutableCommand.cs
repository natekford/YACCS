using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IMutableCommand : IMutableEntityBase, IQueryableCommand
	{
		new IList<IName> Names { get; set; }
		IList<IMutableParameter> Parameters { get; set; }

		ICommand ToCommand();
	}
}