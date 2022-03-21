namespace YACCS.Preconditions.Locked;

/// <summary>
/// Whether or not an item has to be in use.
/// </summary>
public enum Item
{
	/// <summary>
	/// The item being searched for must not be in use.
	/// </summary>
	Unlocked = 0,
	/// <summary>
	/// The item being searched for must be in use.
	/// </summary>
	Locked = 1,
}