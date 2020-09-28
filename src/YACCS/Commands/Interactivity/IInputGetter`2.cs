using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity
{
	public interface IInputGetter<TContext, TInput> where TContext : IContext
	{
		Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
			TContext context,
			IGetInputOptions<TContext, TInput, TValue> options);
	}
}