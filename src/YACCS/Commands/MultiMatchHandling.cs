namespace YACCS.Commands;

/// <summary>
/// Specifies how to handle multiple successful commands matches.
/// </summary>
public enum MultiMatchHandling
{
	/// <summary>
	/// Do not execute any command, return an error.
	/// </summary>
	Error = 0,
	/// <summary>
	/// Execute the command with the highest score.
	/// </summary>
	Best = 1,
}
