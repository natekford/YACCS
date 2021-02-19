using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition : IGroupablePrecondition
	{
		Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			object? value);
	}
}