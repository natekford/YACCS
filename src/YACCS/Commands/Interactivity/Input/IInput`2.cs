using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity.Input
{
	public interface IInput<TContext, TInput> where TContext : IContext
	{
		Task<IInteractivityResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options);
	}
}