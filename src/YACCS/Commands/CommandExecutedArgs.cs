using System;
using System.Collections.Generic;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands;

/// <summary>
/// Provides data about command execution.
/// </summary>
/// <param name="Command">
/// The command that was invoked.
/// </param>
/// <param name="Context">
/// The context which invoked this command.
/// </param>
/// <param name="BeforeExceptions">
/// The exceptions which occurred before execution..
/// </param>
/// <param name="AfterExceptions">
/// The exceptions which occurred after command execution.
/// </param>
/// <param name="DuringException">
/// The exception which occurred during command execution.
/// </param>
/// <param name="Result">
/// <inheritdoc cref="INestedResult.InnerResult" path="/summary" />
/// </param>
public record CommandExecutedArgs(
	IImmutableCommand Command,
	IContext Context,
	IReadOnlyList<Exception>? BeforeExceptions,
	IReadOnlyList<Exception>? AfterExceptions,
	Exception? DuringException,
	IResult Result
) : IResult, INestedResult
{
	IResult INestedResult.InnerResult => Result;
	bool IResult.IsSuccess => Result.IsSuccess;
	string IResult.Response => Result.Response;
}