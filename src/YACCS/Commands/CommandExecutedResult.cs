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
/// <param name="InnerResult">
/// <inheritdoc cref="INestedResult.InnerResult" path="/summary" />
/// </param>
public record CommandExecutedResult(
	IImmutableCommand Command,
	IContext Context,
	IReadOnlyList<Exception>? BeforeExceptions,
	IReadOnlyList<Exception>? AfterExceptions,
	Exception? DuringException,
	IResult InnerResult
) : IResult, INestedResult
{
	bool IResult.IsSuccess => InnerResult.IsSuccess;
	string IResult.Response => InnerResult.Response;
}