using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace YACCS
{
	public class ExceptionEventArgs<T> : HandledEventArgs where T : HandledEventArgs
	{
		public T EventArgs { get; }
		public IReadOnlyList<Exception> Exceptions { get; }

		public ExceptionEventArgs(IReadOnlyList<Exception> exceptions, T eventArgs)
		{
			Exceptions = exceptions;
			EventArgs = eventArgs;
		}

		public ExceptionEventArgs(Exception exception, T originalEventArgs)
		{
			Exceptions = new[] { exception }.ToArray();
			EventArgs = originalEventArgs;
		}
	}
}