using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Results
{
	public class ExecutionResult : INestedResult
	{
		public IImmutableCommand Command { get; }
		public IContext Context { get; }
		public IResult InnerResult { get; }

		public ExecutionResult(IImmutableCommand command, IContext context, IResult result)
		{
			Command = command;
			Context = context;
			InnerResult = result;
		}
	}
}