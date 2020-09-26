namespace YACCS.Results
{
	public interface INestedResult
	{
		IResult InnerResult { get; }
	}
}