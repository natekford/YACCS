using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface ICommand : IEntityBase, IQueryableCommand
	{
		new IList<IName> Names { get; set; }
		IList<IParameter> Parameters { get; set; }

		IEnumerable<IImmutableCommand> ToImmutable();
	}
}