namespace YACCS.Commands.Models;

/// <summary>
/// An entity which supports querying.
/// </summary>
public interface IQueryableEntity
{
	/// <summary>
	/// Objects which contain information about this instance.
	/// These are not all guaranteed to be <see cref="System.Attribute"/>.
	/// </summary>
	IEnumerable<object> Attributes { get; }
}