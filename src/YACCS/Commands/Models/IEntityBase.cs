using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IEntityBase : IQueryableEntity
	{
		new IList<object> Attributes { get; set; }
		string Id { get; set; }
	}
}