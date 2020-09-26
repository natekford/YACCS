using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public interface IParameterPrecondition<in TContext, in TValue>
		: IParameterPrecondition<TValue>
		where TContext : IContext
	{
		Task<IResult> CheckAsync(ParameterInfo parameter, TContext context, [MaybeNull] TValue value);
	}
}