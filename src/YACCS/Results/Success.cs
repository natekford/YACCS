namespace YACCS.Results;

/// <summary>
/// A non-specific result indicating success.
/// </summary>
/// <remarks>
/// Creates a new <see cref="Success"/>.
/// </remarks>
/// <param name="message">
/// <inheritdoc cref="Result.Response" path="/summary"/>
/// </param>
public class Success(string message) : Result(true, message)
{
	/// <summary>
	/// A singleton instance with no message.
	/// </summary>
	public static Success Instance { get; } = new();

	/// <inheritdoc cref="Success(string)"/>
	public Success() : this(string.Empty)
	{
	}
}
