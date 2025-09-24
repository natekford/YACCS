using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

public class Disabled : SummarizablePrecondition<IContext>
{
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IContext context)
		=> new(Result.Failure("Command is disabled."));

	public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new("Command is disabled and is not able to be executed.");
}