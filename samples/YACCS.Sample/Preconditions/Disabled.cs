using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions;

public class Disabled : Precondition<IContext>
{
	public override ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IContext context)
		=> new(Result.Failure("Command is disabled."));
}