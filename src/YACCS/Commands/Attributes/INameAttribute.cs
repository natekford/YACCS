using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute used for setting <see cref="IImmutableParameter.ParameterName"/>.
/// </summary>
public interface INameAttribute
{
	/// <summary>
	/// The name of this entity.
	/// </summary>
	string Name { get; }
}
