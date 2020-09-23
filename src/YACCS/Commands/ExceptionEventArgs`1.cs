using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace YACCS.Commands
{
	public class ExceptionEventArgs<T> : HandledEventArgs where T : HandledEventArgs
	{
		public IReadOnlyList<Exception> Exceptions { get; }
		public T OriginalEventArgs { get; }

		public ExceptionEventArgs(IReadOnlyList<Exception> exceptions, T originalEventArgs)
		{
			Exceptions = exceptions;
			OriginalEventArgs = originalEventArgs;
		}
	}
}