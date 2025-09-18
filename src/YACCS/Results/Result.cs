using System;
using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Results;

/// <summary>
/// The base class of a result.
/// </summary>
/// <param name="isSuccess">
/// <inheritdoc cref="IsSuccess" path="/summary"/>
/// </param>
/// <param name="response">
/// <inheritdoc cref="Response" path="/summary"/>
/// </param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public partial class Result(bool isSuccess, string response) : IResult
{
	/// <inheritdoc />
	public bool IsSuccess { get; } = isSuccess;
	/// <inheritdoc />
	public virtual string Response { get; } = response;
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a failure result.
	/// </summary>
	/// <param name="response">The message of the result.</param>
	/// <returns>A failure result.</returns>
	public static Result Failure(string response)
		=> new(false, response);

	/// <summary>
	/// Creates a success result.
	/// </summary>
	/// <param name="response">The message of the result.</param>
	/// <returns>A success result.</returns>
	public static Result Success(string response)
		=> new(true, response);

	/// <inheritdoc />
	public override string ToString()
		=> Response;
}

/// <summary>
/// Cached results.
/// </summary>
public partial class Result
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
	/// A non-specific result indicating failure.
	/// </summary>
	public static Failure EmptyFailure { get; } = new();
	/// <summary>
	/// A non-specific result indicating success.
	/// </summary>
	public static Success EmptySuccess { get; } = new();
	/// <summary>
	/// An exception occurred after a command was executed.
	/// </summary>
	public static ExceptionAfterCommand ExceptionAfterCommand { get; } = new();
	/// <summary>
	/// An exception occurred while a command was being executed.
	/// </summary>
	public static ExceptionDuringCommand ExceptionDuringCommand { get; } = new();
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
	/// A timed function finished without success or cancellation.
	/// </summary>
	public static TimedOut TimedOut { get; } = new();
	/// <summary>
	/// Too many arguments were provided to the command.
	/// </summary>
	public static TooManyArgs TooManyArgs { get; } = new();
}

/// <summary>
/// Uncached results.
/// </summary>
public partial class Result
{
	/// <summary>
	/// The supplied integer argument was less than the minimum accepted value (inclusive).
	/// </summary>
	/// <param name="min">The minimum accepted value (inclusive).</param>
	/// <returns></returns>
	public static MustBeGreaterThan MustBeGreaterThan(int min) => new(min);

	/// <summary>
	/// The supplied integer argument was greater than the maximum accepted value (inclusive).
	/// </summary>
	/// <param name="max">The maximum accepted value (inclusive).</param>
	/// <returns></returns>
	public static MustBeLessThan MustBeLessThan(int max) => new(max);

	/// <summary>
	/// The supplied argument wasn't in use while the command required it to be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being used.</param>
	/// <returns></returns>
	public static MustBeLocked MustBeLocked(Type type) => new(type);

	/// <summary>
	/// The supplied argument was in use while the command required it to not be in use.
	/// </summary>
	/// <param name="type">The type of item being checked for if it's being unused.</param>
	/// <returns></returns>
	public static MustBeUnlocked MustBeUnlocked(Type type) => new(type);

	/// <summary>
	/// There were multiple values provided with the same argument name.
	/// </summary>
	/// <param name="name">The name of the argument that was provided multiple of.</param>
	/// <returns></returns>
	public static NamedArgDuplicate NamedArgDuplicate(string name) => new(name);

	/// <summary>
	/// A required named argument did not have a value set.
	/// </summary>
	/// <param name="name">The name of the argument that was missing a value.</param>
	/// <returns></returns>
	public static NamedArgMissingValue NamedArgMissingValue(string name) => new(name);

	/// <summary>
	/// There were named arguments provided that do not exist on the class being instantiated.
	/// </summary>
	/// <param name="name">The name of the argument that wasn't found.</param>
	/// <returns></returns>
	public static NamedArgNonExistent NamedArgNonExistent(string name) => new(name);

	/// <summary>
	/// Successfully parsed but failed to find an item of type <paramref name="type"/>.
	/// </summary>
	/// <param name="type">The type that was failed to be found.</param>
	/// <returns></returns>
	public static NotFound NotFound(Type type) => new(type);

	/// <summary>
	/// Failed to parse an item of type <paramref name="type"/>.
	/// </summary>
	/// <param name="type">The type that was failed to be parsed.</param>
	/// <returns></returns>
	public static ParseFailed ParseFailed(Type type) => new(type);

	/// <summary>
	/// Successfully parsed but found too many items of type <paramref name="type"/>.
	/// </summary>
	/// <param name="type">The type that had too many items found.</param>
	/// <returns></returns>
	public static TooManyMatches TooManyMatches(Type type) => new(type);
}