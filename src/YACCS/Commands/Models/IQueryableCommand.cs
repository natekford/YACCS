using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IQueryableCommand : IQueryableEntity
	{
		IEnumerable<IName> Names { get; }
	}
}