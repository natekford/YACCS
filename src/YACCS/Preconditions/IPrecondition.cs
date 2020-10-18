using System;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition
	{
		Task AfterExecutionAsync(IImmutableCommand command, IContext context, Exception? exception);

		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);

		Task<IResult> CheckAsync(IImmutableCommand command, IContext context);
	}
}