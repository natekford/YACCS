using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Preconditions
{
	public abstract class ParameterPrecondition<TContext, TValue>
		: GroupablePrecondition, IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		public abstract Task<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			TContext context,
			[MaybeNull] TValue value);

		Task<IResult> IParameterPrecondition.CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<TContext, TValue>(command, parameter, context, value, CheckAsync);

		Task<IResult> IParameterPrecondition<TValue>.CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] TValue value)
			=> this.CheckAsync<TContext, TValue>(command, parameter, context, value, CheckAsync);
	}
}