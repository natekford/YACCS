namespace YACCS.Preconditions;

/// <summary>
/// The operator to use for a <see cref="IGroupablePrecondition"/>.
/// </summary>
public enum Op
{
	/// <summary>
	/// ALL preconditions with this operator must succeed for the command to be valid.
	/// </summary>
	And = 1,
	/// <summary>
	/// ONE OF the preconditions with this operator must succeed for the command to be valid.
	/// </summary>
	Or = 2,
	// Don't include NOT since getting the "correct" failure message would be a pain
	// Not = 3,
}