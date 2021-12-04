namespace YACCS.Results;

/// <summary>
/// Defines an inner result so pattern matching can work.
/// </summary>
public interface INestedResult
{
	/// <summary>
	/// The result this is wrapping.
	/// </summary>
	IResult InnerResult { get; }
}