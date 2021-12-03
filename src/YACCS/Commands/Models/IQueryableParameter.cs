namespace YACCS.Commands.Models;

/// <summary>
/// A parameter which supports querying.
/// </summary>
public interface IQueryableParameter : IQueryableEntity
{
	/// <summary>
	/// The original parameter name.
	/// For the case of a <see cref="Parameter"/> created via
	/// reflection, this will be the reflected name.
	/// </summary>
	string OriginalParameterName { get; }
	/// <summary>
	/// The type of parameter this is.
	/// </summary>
	Type ParameterType { get; }
}
