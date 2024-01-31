namespace YACCS.Results;

/// <summary>
/// A non-specific result indicating failure.
/// </summary>
/// <remarks>
/// Creates a new <see cref="Failure"/>.
/// </remarks>
/// <param name="message">
/// <inheritdoc cref="Result.Response" path="/summary"/>
/// </param>
public class Failure(string message) : Result(false, message)
{
	/// <summary>
	/// A singleton instance with no message.
	/// </summary>
	public static Failure Instance { get; } = new();

	/// <inheritdoc cref="Failure(string)"/>
	public Failure() : this(string.Empty)
	{
	}
}
