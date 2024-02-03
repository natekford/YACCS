using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute used for setting <see cref="IImmutableEntity.PrimaryId"/>.
/// </summary>
public interface IIdAttribute
{
	/// <summary>
	/// The id to use.
	/// </summary>
	string Id { get; }
}