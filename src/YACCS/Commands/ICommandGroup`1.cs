using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandGroup<in TContext> : ICommandGroup where TContext : IContext
	{
		Task AfterExecutionAsync(IImmutableCommand command, TContext context, IResult result);

		Task BeforeExecutionAsync(IImmutableCommand command, TContext context);
	}
}