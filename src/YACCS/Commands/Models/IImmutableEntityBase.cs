using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IImmutableEntityBase : IQueryableEntity
	{
		new IReadOnlyList<object> Attributes { get; }
		string Id { get; }
	}
}