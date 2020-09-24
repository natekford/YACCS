using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.ParameterPreconditions
{
	public abstract class ParameterPrecondition<TContext, TValue>
		: IParameterPrecondition<TContext, TValue> where TContext : IContext
	{
		public abstract Task<IResult> CheckAsync(CommandInfo info, TContext context, [MaybeNull] TValue value);

		public Task<IResult> CheckAsync(CommandInfo info, IContext context, object? value)
		{
			if (!(value is TValue castedValue))
			{
				return InvalidParameterResult.InstanceTask;
			}
			return CheckAsync(info, context, castedValue);
		}

		public Task<IResult> CheckAsync(CommandInfo info, IContext context, [MaybeNull] TValue value)
		{
			if (!(context is TContext castedContext))
			{
				return InvalidContextResult.InstanceTask;
			}
			return CheckAsync(info, castedContext, value);
		}
	}
}