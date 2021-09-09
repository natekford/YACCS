using System;
using System.Collections.Generic;

namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a groupable precondition.
	/// </summary>
	public abstract class GroupablePrecondition : IGroupablePrecondition
	{
		/// <inheritdoc />
		public virtual IReadOnlyList<string> Groups { get; } = Array.Empty<string>();
		/// <inheritdoc />
		public virtual BoolOp Op { get; } = BoolOp.And;
	}
}