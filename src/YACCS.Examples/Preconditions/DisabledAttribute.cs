
using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions
{
	public class DisabledAttribute<TContext> : Precondition<TContext>
		where TContext : IContext
	{
		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			TContext context)
			=> new(new FailureResult("Command is disabled."));
	}
}