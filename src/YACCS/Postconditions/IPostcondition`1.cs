using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Postconditions
{
	public interface IPostcondition<in TContext> : IPostcondition
		where TContext : IContext
	{
		Task AfterExecutionAsync(IImmutableCommand command, TContext context);
	}
}