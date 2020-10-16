using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public interface IParameterPrecondition
	{
		Task<IResult> CheckAsync(ParameterInfo parameter, IContext context, object? value);
	}
}