using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Interactivity.Input
{
	/// <summary>
	/// Defines a method for getting a value from a source of input.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TInput"></typeparam>
	public interface IInput<TContext, TInput> where TContext : IContext
	{
		/// <summary>
		/// Gets the next successful <typeparamref name="TValue"/> that can be parsed
		/// from any <typeparamref name="TInput"/> received.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="context">The context which initialized getting input.</param>
		/// <param name="options">The options to use when getting input.</param>
		/// <returns>A result indicating success or failure.</returns>
		Task<ITypeReaderResult<TValue>> GetAsync<TValue>(
			TContext context,
			IInputOptions<TContext, TInput, TValue> options);
	}
}