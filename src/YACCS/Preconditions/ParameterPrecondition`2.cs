using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// The base class for a parameter precondition.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public abstract class ParameterPrecondition<TContext, TValue>
		: GroupablePrecondition, IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		/// <inheritdoc />
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