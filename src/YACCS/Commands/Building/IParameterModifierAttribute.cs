using YACCS.Commands.Models;

namespace YACCS.Commands.Building;

/// <summary>
/// An attribute to modify <see cref="IParameter"/> during creation.
/// </summary>
public interface IParameterModifierAttribute
{
	/// <summary>
	/// Modifies the passed in parameter.
	/// </summary>
	/// <param name="parameter">The parameter to modify.</param>
	void ModifyParameter(IParameter parameter);
}
