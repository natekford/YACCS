namespace YACCS.Help.Attributes;

/// <summary>
/// A numbered id to easily search for commands with.
/// </summary>
public interface IRuntimeCommandId
{
	/// <summary>
	/// The id of this command.
	/// </summary>
	int Id { get; }
}