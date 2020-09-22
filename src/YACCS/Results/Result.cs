using System.Diagnostics;

namespace YACCS.Results
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Result : IResult
	{
		public bool IsSuccess { get; }
		public string Response { get; }
		private string DebuggerDisplay => $"IsSuccess = {IsSuccess}, Response = {Response}";

		public Result(bool isSuccess, string response)
		{
			IsSuccess = isSuccess;
			Response = response;
		}

		public static IResult FromError(string response)
			=> new Result(false, response);

		public static IResult FromSuccess()
			=> new Result(true, "");
	}
}