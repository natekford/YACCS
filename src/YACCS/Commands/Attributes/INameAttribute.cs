using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	/// <summary>
	/// An attribute used for setting <see cref="IImmutableParameter.ParameterName"/>.
	/// </summary>
	public interface INameAttribute
	{
		/// <summary>
		/// The name to use.
		/// </summary>
		string Name { get; }
	}
}