using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace YACCS.Commands
{
	public static class ExceptionEventArgsUtils
	{
		public static ExceptionEventArgs<T> WithExceptions<T>(this T e, params Exception[] exs)
			where T : HandledEventArgs
			=> new ExceptionEventArgs<T>(exs, e);

		public static ExceptionEventArgs<T> WithExceptions<T>(this T e, IReadOnlyList<Exception> exs)
			where T : HandledEventArgs
			=> new ExceptionEventArgs<T>(exs, e);
	}

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