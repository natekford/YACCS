namespace YACCS.Results
{
	public interface IResult
	{
		bool IsSuccess { get; }
		string Response { get; }
	}
}