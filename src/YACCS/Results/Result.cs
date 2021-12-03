using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Results;

/// <summary>
/// The base class of a result.
/// </summary>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public abstract class Result : IResult
{
	/// <inheritdoc />
	public bool IsSuccess { get; }
	/// <inheritdoc />
	public virtual string Response { get; }
	private string DebuggerDisplay => this.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="Result"/>.
	/// </summary>
	/// <param name="isSuccess">
	/// <inheritdoc cref="IsSuccess" path="/summary"/>
	/// </param>
	/// <param name="response">
	/// <inheritdoc cref="Response" path="/summary"/>
	/// </param>
	protected Result(bool isSuccess, string response)
	{
		IsSuccess = isSuccess;
		Response = response;
	}

	/// <inheritdoc />
	public override string ToString()
		=> Response;
}