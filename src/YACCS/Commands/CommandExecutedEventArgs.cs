using System.ComponentModel;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Provides data about command execution.
/// </summary>
/// <remarks>
/// Creates a new <see cref="CommandExecutedEventArgs"/>.
/// </remarks>
/// <param name="command">
/// <inheritdoc cref="Command" path="/summary"/>
/// </param>
/// <param name="context">
/// <inheritdoc cref="Context" path="/summary"/>
/// </param>
/// <param name="beforeExceptions">
/// <inheritdoc cref="BeforeExceptions" path="/summary"/>
/// </param>
/// <param name="afterExceptions">
/// <inheritdoc cref="AfterExceptions" path="/summary"/>
/// </param>
/// <param name="duringException">
/// <inheritdoc cref="DuringException" path="/summary"/>
/// </param>
/// <param name="result">
/// <inheritdoc cref="Result" path="/summary"/>
/// </param>
public class CommandExecutedEventArgs(
	IImmutableCommand command,
	IContext context,
	IReadOnlyList<Exception>? beforeExceptions,
	IReadOnlyList<Exception>? afterExceptions,
	Exception? duringException,
	IResult result
) : HandledEventArgs, IResult, INestedResult
{
	/// <summary>
	/// The exceptions which occurred after command execution.
	/// </summary>
	public IReadOnlyList<Exception>? AfterExceptions { get; } = afterExceptions;
	/// <summary>
	/// The exceptions which occurred before execution..
	/// </summary>
	public IReadOnlyList<Exception>? BeforeExceptions { get; } = beforeExceptions;
	/// <summary>
	/// The command that was invoked.
	/// </summary>
	public IImmutableCommand Command { get; } = command;
	/// <summary>
	/// The context which invoked this command.
	/// </summary>
	public IContext Context { get; } = context;
	/// <summary>
	/// The exception which occurred during command execution.
	/// </summary>
	public Exception? DuringException { get; } = duringException;
	/// <inheritdoc cref="INestedResult.InnerResult" />
	public IResult Result { get; } = result;
	IResult INestedResult.InnerResult => Result;
	bool IResult.IsSuccess => Result.IsSuccess;
	string IResult.Response => Result.Response;
}