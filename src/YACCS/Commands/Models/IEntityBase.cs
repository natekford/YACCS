namespace YACCS.Commands.Models;

/// <summary>
/// The base interface for most command objects.
/// </summary>
public interface IEntityBase : IQueryableEntity
{
	/// <inheritdoc cref="IQueryableEntity.Attributes" />
	new IList<object> Attributes { get; set; }
}
