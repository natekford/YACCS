using System;

namespace YACCS.Commands
{
	public class InvalidCommandTypeException : Exception
	{
		public InvalidCommandTypeException() : base()
		{
		}

		public InvalidCommandTypeException(string message) : base(message)
		{
		}

		public InvalidCommandTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}