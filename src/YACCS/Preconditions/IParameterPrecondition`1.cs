using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition<in TValue> : IParameterPrecondition
	{
		ValueTask<IResult> CheckAsync(CommandMeta meta, IContext context, TValue? value);
	}
}