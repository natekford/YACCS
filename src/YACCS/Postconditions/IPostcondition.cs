using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Postconditions
{
	public interface IPostcondition
	{
		Task AfterExecutionAsync(IImmutableCommand command, IContext context);
	}
}