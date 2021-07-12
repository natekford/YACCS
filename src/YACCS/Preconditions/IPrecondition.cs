using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition : IGroupablePrecondition
	{
		ValueTask AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception);

		ValueTask BeforeExecutionAsync(IImmutableCommand command, IContext context);

		ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}