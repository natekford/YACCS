using System.ComponentModel;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Provides data about command execution.
/// </summary>
public class CommandExecutedEventArgs : HandledEventArgs, IResult, INestedResult
{
	/// <summary>
	/// The exceptions which occurred after command execution.
	/// </summary>
	public IReadOnlyList<Exception>? AfterExceptions { get; }
	/// <summary>
	/// The exceptions which occurred before execution..
	/// </summary>
	public IReadOnlyList<Exception>? BeforeExceptions { get; }
	/// <summary>
	/// The command that was invoked.
	/// </summary>
	public IImmutableCommand Command { get; }
	/// <summary>
	/// The context which invoked this command.
	/// </summary>
	public IContext Context { get; }
	/// <summary>
	/// The exception which occurred during command execution.
	/// </summary>
	public Exception? DuringException { get; }
	/// <inheritdoc />
	public IResult Result { get; }
	IResult INestedResult.InnerResult => Result;
	bool IResult.IsSuccess => Result.IsSuccess;
	string IResult.Response => Result.Response;

	/// <summary>
	/// Creates a new <see cref="CommandExecutedEventArgs"/>.
	/// </summary>
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
	public CommandExecutedEventArgs(
		IImmutableCommand command,
		IContext context,
		IReadOnlyList<Exception>? beforeExceptions,
		IReadOnlyList<Exception>? afterExceptions,
		Exception? duringException,
		IResult result)
	{
		AfterExceptions = afterExceptions;
		BeforeExceptions = beforeExceptions;
		Command = command;
		Context = context;
		DuringException = duringException;
		Result = result;
	}
}
