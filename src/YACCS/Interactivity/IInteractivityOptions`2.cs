
using YACCS.Commands;

namespace YACCS.Interactivity
{
	/// <summary>
	/// Interactivity options which support validating input.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TInput"></typeparam>
	public interface IInteractivityOptions<in TContext, in TInput>
		: IInteractivityOptions
		where TContext : IContext
	{
		/// <summary>
		/// Criteria for determining if an input source is valid.
		/// </summary>
		IEnumerable<ICriterion<TContext, TInput>> Criteria { get; }
	}
}