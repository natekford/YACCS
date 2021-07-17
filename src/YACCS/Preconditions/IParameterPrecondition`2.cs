using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public interface IParameterPrecondition<in TContext, in TValue>
		: IParameterPrecondition<TValue>
		where TContext : IContext
	{
		ValueTask<IResult> CheckAsync(CommandMeta meta, TContext context, TValue? value);
	}
}