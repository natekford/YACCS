using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace YACCS
{
	public static class ExceptionEventArgsUtils
	{
		public static ExceptionEventArgs<T> WithExceptions<T>(this T e, params Exception[] exs)
			where T : HandledEventArgs
			=> new(exs, e);

		public static ExceptionEventArgs<T> WithExceptions<T>(this T e, IReadOnlyList<Exception> exs)
			where T : HandledEventArgs
			=> new(exs, e);
	}
}