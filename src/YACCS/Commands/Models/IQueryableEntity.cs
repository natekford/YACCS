using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IQueryableCommand : IQueryableEntity
	{
		IEnumerable<IName> Names { get; }
	}

	public interface IQueryableEntity
	{
		IEnumerable<object> Attributes { get; }
	}
}