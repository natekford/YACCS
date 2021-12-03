namespace YACCS.Preconditions.InUse;

/// <summary>
/// Whether or not an item has to be in use.
/// </summary>
public enum Item
{
	/// <summary>
	/// The item being searched for must not be in use.
	/// </summary>
	MustNotBeInUse = 0,
	/// <summary>
	/// The item being searched for must be in use.
	/// </summary>
	MustBeInUser = 1,
}