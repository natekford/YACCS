using YACCS.Localization;

namespace YACCS.Results;

/// <summary>
/// Cached results.
/// </summary>
public static class CachedResults
{
	/// <summary>
	/// Something was canceled.
	/// </summary>
	public static LocalizedResult Canceled { get; }
		= new(false, Keys.CanceledResult);
	/// <summary>
	/// A command was unable to be found.
	/// </summary>
	public static LocalizedResult CommandNotFound { get; }
		= new(false, Keys.CommandNotFoundResult);
	/// <summary>
	/// An exception occurred after a command was executed.
	/// </summary>
	public static LocalizedResult ExceptionAfterCommand { get; }
		= new(false, Keys.ExceptionAfterCommandResult);
	/// <summary>
	/// An exception occurred while a command was being executed.
	/// </summary>
	public static LocalizedResult ExceptionDuringCommand { get; }
		= new(false, Keys.ExceptionDuringCommandResult);
	/// <summary>
	/// A non-specific result indicating failure.
	/// </summary>
	public static Result Failure { get; }
		= new(false, string.Empty);
	/// <summary>
	/// An interaction ended (not canceled or timed out).
	/// </summary>
	public static LocalizedResult InteractionEnded { get; }
		= new(false, Keys.InteractionEndedResult);
	/// <summary>
	/// The supplied context is not valid for the command attempting to be executed.
	/// </summary>
	public static LocalizedResult InvalidContext { get; }
		= new(false, Keys.InvalidContextResult);
	/// <summary>
	/// The passed in parameter is not the correct type for the parameter precondition
	/// receiving it.
	/// </summary>
	public static LocalizedResult InvalidParameter { get; }
		= new(false, Keys.InvalidParameterResult);
	/// <summary>
	/// Multiple commands matched the passed in arguments and the command service was configured
	/// to treat this as an error.
	/// </summary>
	public static LocalizedResult MultiMatchHandlingError { get; }
		= new(false, Keys.MultiMatchHandlingErrorResult);
	/// <summary>
	/// There was an odd number of arguments supplied to a named argument parameter.
	/// </summary>
	public static LocalizedResult NamedArgBadCount { get; }
		= new(false, Keys.NamedArgBadCountResult);
	/// <summary>
	/// The passed in dictionary for named argument command execution is an invalid type.
	/// </summary>
	public static LocalizedResult NamedArgInvalidDictionary { get; }
		= new(false, Keys.NamedArgInvalidDictionaryResult);
	/// <summary>
	/// Not enough arguments were provided to the command.
	/// </summary>
	public static LocalizedResult NotEnoughArgs { get; }
		= new(false, Keys.NotEnoughArgsResult);
	/// <summary>
	/// A parameter was null when it should not have been.
	/// </summary>
	public static LocalizedResult NullParameter { get; }
		= new(false, Keys.NullParameterResult);
	/// <summary>
	/// Provided string had quotes that were unable to be parsed.
	/// </summary>
	public static LocalizedResult QuoteMismatch { get; }
		= new(false, Keys.QuoteMismatchResult);
	/// <summary>
	/// A non-specific result indicating success.
	/// </summary>
	public static Result Success { get; }
		= new(true, string.Empty);
	/// <summary>
	/// A timed function finished without success or cancellation.
	/// </summary>
	public static LocalizedResult TimedOut { get; }
		= new(false, Keys.TimedOutResult);
	/// <summary>
	/// Too many arguments were provided to the command.
	/// </summary>
	public static LocalizedResult TooManyArgs { get; }
		= new(false, Keys.TooManyArgsResult);
}