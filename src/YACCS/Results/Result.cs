using System.Diagnostics;

namespace YACCS.Results
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class Result : IResult
	{
		public bool IsSuccess { get; }
		public string Response { get; }
		private string DebuggerDisplay => $"IsSuccess = {IsSuccess}, Response = {Response}";

		protected Result(bool isSuccess, string response)
		{
			IsSuccess = isSuccess;
			Response = response;
		}
	}
}