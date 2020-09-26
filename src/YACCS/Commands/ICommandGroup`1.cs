using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{
	public interface ICommandGroup<in TContext> : ICommandGroup where TContext : IContext
	{
		Task AfterExecutionAsync(IImmutableCommand command, TContext context);

		Task BeforeExecutionAsync(IImmutableCommand command, TContext context);
	}
}