using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition<in TContext> : IPrecondition where TContext : IContext
	{
		Task<IResult> CheckAsync(CommandInfo info, TContext context);
	}
}