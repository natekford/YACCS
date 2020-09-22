using System;

namespace YACCS.Results
{
	public class ExceptionResult : Result
	{
		public Exception Exception { get; }

		public ExceptionResult(Exception exception) : base(false, exception.Message)
		{
			Exception = exception;
		}
	}
}