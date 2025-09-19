using System.Collections.Generic;

namespace YACCS.Commands.Models;

/// <summary>
/// The base interface for most immutable command objects.
/// </summary>
public interface IImmutableEntity : IQueryableEntity
{
	/// <inheritdoc cref="IQueryableEntity.Attributes" />
	new IReadOnlyList<AttributeInfo> Attributes { get; }
	/// <summary>
	/// The primary id of this entity.
	/// </summary>
	string PrimaryId { get; }
}