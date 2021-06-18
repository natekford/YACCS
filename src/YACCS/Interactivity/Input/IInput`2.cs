using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input
{
	public interface IInput<TContext, TInput> where TContext : IContext
	{
		Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options);
	}
}