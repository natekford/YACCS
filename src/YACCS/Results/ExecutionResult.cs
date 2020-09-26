using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Results
{
	public class ExecutionResult : Result, INestedResult
	{
		public IImmutableCommand Command { get; }
		public IContext Context { get; }
		public IResult InnerResult { get; }

		public ExecutionResult(IImmutableCommand command, IContext context, IResult result)
			: base(result.IsSuccess, result.Response)
		{
			Command = command;
			Context = context;
			InnerResult = result;
		}
	}
}