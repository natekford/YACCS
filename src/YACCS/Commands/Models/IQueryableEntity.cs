using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IQueryableEntity
	{
		IEnumerable<object> Attributes { get; }
	}
}