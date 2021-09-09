namespace YACCS.Results
{
	/// <summary>
	/// Defines an inner result so pattern matching can work.
	/// </summary>
	public interface INestedResult
	{
		/// <summary>
		/// The actual result.
		/// </summary>
		IResult InnerResult { get; }
	}
}