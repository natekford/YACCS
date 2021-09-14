
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Interactivity
{
	/// <summary>
	/// Defines a method for determining if an input is valid.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TInput"></typeparam>
	public interface ICriterion<in TContext, in TInput> where TContext : IContext
	{
		/// <summary>
		/// Determines if <paramref name="input"/> is valid.
		/// </summary>
		/// <param name="context">The context which initialized getting input.</param>
		/// <param name="input">The current received input.</param>
		/// <returns>A result indicating success or failure.</returns>
		ValueTask<IResult> JudgeAsync(TContext context, TInput input);
	}
}