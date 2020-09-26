using System;

namespace YACCS.Results
{
	public class ResultException : Exception
	{
		public ResultException() : base()
		{
		}

		public ResultException(string message) : base(message)
		{
		}

		public ResultException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}