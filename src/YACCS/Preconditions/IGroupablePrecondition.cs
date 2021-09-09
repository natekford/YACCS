using System.Collections.Generic;

namespace YACCS.Preconditions
{
	/// <summary>
	/// Defines properties for a precondition which can be grouped.
	/// </summary>
	public interface IGroupablePrecondition
	{
		/// <summary>
		/// The groups this precondition belongs to.
		/// </summary>
		IReadOnlyList<string> Groups { get; }
		/// <summary>
		/// The boolean operator to use for this precondition.
		/// </summary>
		BoolOp Op { get; }
	}
}