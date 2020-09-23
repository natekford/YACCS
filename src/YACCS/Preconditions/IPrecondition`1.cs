using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition<in TContext> : IPrecondition where TContext : IContext
	{
		Task<IResult> CheckAsync(TContext context, IImmutableCommand command);
	}
}