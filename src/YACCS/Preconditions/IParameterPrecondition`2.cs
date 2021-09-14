
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <inheritdoc />
	public interface IParameterPrecondition<in TContext, in TValue>
		: IParameterPrecondition<TValue>
		where TContext : IContext
	{
		/// <inheritdoc cref="IParameterPrecondition.CheckAsync(CommandMeta, IContext, object?)"/>
		ValueTask<IResult> CheckAsync(CommandMeta meta, TContext context, TValue? value);
	}
}