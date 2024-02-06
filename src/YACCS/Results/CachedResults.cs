namespace YACCS.Results;

/// <summary>
/// Cached results.
/// </summary>
public static class CachedResults
{
	/// <summary>
	/// Something was canceled.
	/// </summary>
	public static Canceled Canceled { get; } = new();
	/// <summary>
	/// A command was unable to be found.
	/// </summary>
	public static CommandNotFound CommandNotFound { get; } = new();
	/// <summary>
	/// An exception occurred after a command was executed.
	/// </summary>
	public static ExceptionAfterCommand ExceptionAfterCommand { get; } = new();
	/// <summary>
	/// An exception occurred while a command was being executed.
	/// </summary>
	public static ExceptionDuringCommand ExceptionDuringCommand { get; } = new();
	/// <summary>
	/// A non-specific result indicating failure.
	/// </summary>
	public static Failure Failure { get; } = new();
	/// <summary>
	/// An interaction ended (not canceled or timed out).
	/// </summary>
	public static InteractionEnded InteractionEnded { get; } = new();
	/// <summary>
	/// The supplied context is not valid for the command attempting to be executed.
	/// </summary>
	public static InvalidContext InvalidContext { get; } = new();
	/// <summary>
	/// The passed in parameter is not the correct type for the parameter precondition
	/// receiving it.
	/// </summary>
	public static InvalidParameter InvalidParameter { get; } = new();
	/// <summary>
	/// Multiple commands matched the passed in arguments and the command service was configured
	/// to treat this as an error.
	/// </summary>
	public static MultiMatchHandlingError MultiMatchHandlingError { get; } = new();
	/// <summary>
	/// There was an odd number of arguments supplied to a named argument parameter.
	/// </summary>
	public static NamedArgBadCount NamedArgBadCount { get; } = new();
	/// <summary>
	/// The passed in dictionary for named argument command execution is an invalid type.
	/// </summary>
	public static NamedArgInvalidDictionary NamedArgInvalidDictionary { get; } = new();
	/// <summary>
	/// Not enough arguments were provided to the command.
	/// </summary>
	public static NotEnoughArgs NotEnoughArgs { get; } = new();
	/// <summary>
	/// A parameter was null when it should not have been.
	/// </summary>
	public static NullParameter NullParameter { get; } = new();
	/// <summary>
	/// Provided string had quotes that were unable to be parsed.
	/// </summary>
	public static QuoteMismatch QuoteMismatch { get; } = new();
	/// <summary>
	/// A non-specific result indicating success.
	/// </summary>
	public static Success Success { get; } = new();
	/// <summary>
	/// A timed function finished without success or cancellation.
	/// </summary>
	public static TimedOut TimedOut { get; } = new();
	/// <summary>
	/// Too many arguments were provided to the command.
	/// </summary>
	public static TooManyArgs TooManyArgs { get; } = new();
}