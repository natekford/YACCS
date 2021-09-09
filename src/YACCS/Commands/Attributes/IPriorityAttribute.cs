using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes
{
	/// <summary>
	/// An attribute for setting <see cref="IImmutableCommand.Priority"/>
	/// </summary>
	public interface IPriorityAttribute
	{
		/// <summary>
		/// The priority to use when determining the best command.
		/// Higher priority is better.
		/// </summary>
		int Priority { get; }
	}
}