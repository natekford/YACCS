namespace YACCS.Results;

/// <summary>
/// Defines the properties of a result.
/// </summary>
public interface IResult
{
	/// <summary>
	/// Whether or not this result indicates success.
	/// </summary>
	bool IsSuccess { get; }
	/// <summary>
	/// The text representing this result.
	/// </summary>
	string Response { get; }
}
