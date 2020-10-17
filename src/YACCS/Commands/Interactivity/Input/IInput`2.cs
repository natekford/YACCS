using System.Threading.Tasks;

using YACCS.TypeReaders;

namespace YACCS.Commands.Interactivity.Input
{
	public interface IInput<TContext, TInput> where TContext : IContext
	{
		Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options);
	}
}