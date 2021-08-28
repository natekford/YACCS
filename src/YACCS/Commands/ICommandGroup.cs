using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandGroup
	{
		Task AfterExecutionAsync(IImmutableCommand command, IContext context, IResult result);

		Task BeforeExecutionAsync(IImmutableCommand command, IContext context);
	}
}