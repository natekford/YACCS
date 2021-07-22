using System.Diagnostics;

using YACCS.Commands;

namespace YACCS.Results
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public abstract class Result : IResult
	{
		public bool IsSuccess { get; }
		public virtual string Response { get; }
		private string DebuggerDisplay => this.FormatForDebuggerDisplay();

		protected Result(bool isSuccess, string response)
		{
			IsSuccess = isSuccess;
			Response = response;
		}

		public override string ToString()
			=> Response;
	}
}