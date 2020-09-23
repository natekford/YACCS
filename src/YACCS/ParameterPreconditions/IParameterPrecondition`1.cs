using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public interface IParameterPrecondition<in TValue> : IParameterPrecondition
	{
		Task<IResult> CheckAsync(IContext context, [MaybeNull] TValue value);
	}
}