namespace YACCS.Commands;

/// <summary>
/// The stage of a command being parsed.
/// </summary>
public enum CommandStage : int
{
	//QuoteMismatch = -1,
	//NotFound = 0,
	/// <summary>
	/// Invalid context type.
	/// </summary>
	BadContext = 1,
	/// <summary>
	/// Invalid argument count.
	/// </summary>
	BadArgCount = 2,
	//CorrectArgCount = 3,
	/// <summary>
	/// Command did not succeed every precondition.
	/// </summary>
	FailedPrecondition = 4,
	/// <summary>
	/// Argument could not be parsed.
	/// </summary>
	FailedTypeReader = 5,
	/// <summary>
	/// Argument did not succeed every precondition.
	/// </summary>
	FailedParameterPrecondition = 6,
	//FailedOptionalArgs = 7,
	/// <summary>
	/// Command is allowed.
	/// </summary>
	CanExecute = 8,
}
