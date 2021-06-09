using System;
using System.Collections.Generic;
using System.ComponentModel;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public class CommandExecutedEventArgs : HandledEventArgs, IResult, INestedResult
	{
		public IReadOnlyList<Exception>? AfterExceptions { get; }
		public IReadOnlyList<Exception>? BeforeExceptions { get; }
		public IImmutableCommand Command { get; }
		public IContext Context { get; }
		public Exception? DuringException { get; }
		public IResult Result { get; }
		IResult INestedResult.InnerResult => Result;
		bool IResult.IsSuccess => Result.IsSuccess;
		string IResult.Response => Result.Response;

		public CommandExecutedEventArgs(
			IImmutableCommand command,
			IContext context,
			IReadOnlyList<Exception>? beforeExceptions,
			IReadOnlyList<Exception>? afterExceptions,
			Exception? duringException,
			IResult result)
		{
			AfterExceptions = afterExceptions;
			BeforeExceptions = beforeExceptions;
			Command = command;
			Context = context;
			DuringException = duringException;
			Result = result;
		}
	}
}