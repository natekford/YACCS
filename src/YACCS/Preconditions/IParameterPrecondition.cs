using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition : IGroupablePrecondition
	{
		ValueTask<IResult> CheckAsync(CommandMeta meta, IContext context, object? value);
	}
}