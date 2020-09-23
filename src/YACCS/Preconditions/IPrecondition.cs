using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition
	{
		Task<IResult> CheckAsync(IContext context, IImmutableCommand command);
	}
}