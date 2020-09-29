using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity.Input
{
	public interface IInputGetter<TContext, TInput> where TContext : IContext
	{
		Task<IInteractiveResult<TValue>> GetInputAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options);
	}
}