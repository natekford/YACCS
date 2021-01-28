using System;

namespace YACCS.Parsing
{

	public class QuoteMismatchException : ArgumentException
	{
		public QuoteMismatchException()
		{
		}

		public QuoteMismatchException(string message) : base(message)
		{
		}

		public QuoteMismatchException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public QuoteMismatchException(string message, string paramName) : base(message, paramName)
		{
		}

		public QuoteMismatchException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
		{
		}
	}
}