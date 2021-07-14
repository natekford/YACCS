using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class ParameterPrecondition<TContext, TValue>
		: GroupablePrecondition, IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		public abstract ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			TContext context,
			TValue? value);

		ValueTask<IResult> IParameterPrecondition.CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<TContext, TValue>(command, parameter, context, value, CheckAsync);

		ValueTask<IResult> IParameterPrecondition<TValue>.CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			TValue? value)
			=> this.CheckAsync<TContext, TValue>(command, parameter, context, value, CheckAsync);
	}
}