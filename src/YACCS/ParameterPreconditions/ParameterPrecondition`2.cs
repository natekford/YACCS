using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public abstract class ParameterPrecondition<TContext, TValue>
		: IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		public abstract Task<IResult> CheckAsync(ParameterInfo parameter, TContext context, [MaybeNull] TValue value);

		Task<IResult> IParameterPrecondition.CheckAsync(ParameterInfo parameter, IContext context, object? value)
			=> this.CheckAsync<TContext, TValue>(parameter, context, value, CheckAsync);

		Task<IResult> IParameterPrecondition<TValue>.CheckAsync(ParameterInfo parameter, IContext context, [MaybeNull] TValue value)
			=> this.CheckAsync<TContext, TValue>(parameter, context, value, CheckAsync);
	}
}