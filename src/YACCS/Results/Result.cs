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
public class Result(bool isSuccess, string response) : IResult
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