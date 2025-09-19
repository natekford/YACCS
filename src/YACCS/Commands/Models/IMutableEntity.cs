using System.Collections.Generic;

namespace YACCS.Commands.Models;

/// <summary>
/// The base interface for most command objects.
/// </summary>
public interface IMutableEntity : IQueryableEntity
{
	/// <inheritdoc cref="IQueryableEntity.Attributes" />
	new IList<AttributeInfo> Attributes { get; set; }
}