using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Results
{
	public class ExecutionResult : Result, INestedResult
	{
		public ICommand Command { get; }
		public IContext Context { get; }
		public IResult Result { get; }

		public ExecutionResult(ICommand command, IContext context, IResult result)
			: base(result.IsSuccess, result.Response)
		{
			Command = command;
			Context = context;
			Result = result;
		}
	}
}