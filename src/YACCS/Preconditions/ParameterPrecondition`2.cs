using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class ParameterPrecondition<TContext, TValue>
		: GroupablePrecondition, IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		public abstract ValueTask<IResult> CheckAsync(CommandMeta meta, TContext context, TValue? value);

		ValueTask<IResult> IParameterPrecondition.CheckAsync(
			CommandMeta meta,
			IContext context,
			object? value)
			=> this.CheckAsync<TContext, TValue>(meta, context, value, CheckAsync);

		ValueTask<IResult> IParameterPrecondition<TValue>.CheckAsync(
			CommandMeta meta,
			IContext context,
			TValue? value)
			=> this.CheckAsync<TContext, TValue>(meta, context, value, CheckAsync);
	}
}