namespace YACCS.Results;

/// <summary>
/// Boxes a value so it can be returned as a result.
/// </summary>
/// <remarks>
/// Creates a new <see cref="ValueResult"/>.
/// </remarks>
/// <param name="value">
/// <inheritdoc cref="Value" path="/summary"/>
/// </param>
public class ValueResult(object? value) : Result(true, value?.ToString() ?? "")
{
	/// <summary>
	/// The value of this result.
	/// </summary>
	public object? Value { get; } = value;
}