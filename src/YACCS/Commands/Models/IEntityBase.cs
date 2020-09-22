using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IEntityBase : IQueryableEntity
	{
		new IReadOnlyList<object> Attributes { get; }
		string Id { get; }
	}
}