using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public interface IParameterPrecondition
	{
		Task<IResult> CheckAsync(CommandInfo info, IContext context, object? value);
	}
}