using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IPrecondition
	{
		Task<IResult> CheckAsync(CommandInfo info, IContext context);
	}
}