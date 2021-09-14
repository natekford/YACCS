
using YACCS.Commands;
using YACCS.Results;

namespace YACCS.Preconditions
{
	/// <summary>
	/// Defines a method for validating a parameter precondition.
	/// </summary>
	public interface IParameterPrecondition : IGroupablePrecondition
	{
		/// <summary>
		/// Determines if <paramref name="context"/> and <paramref name="value"/> are valid
		/// for this precondition.
		/// </summary>
		/// <param name="meta">The command and parameter combination.</param>
		/// <param name="context">The context invoking a command.</param>
		/// <param name="value">The value to use for this parameter.</param>
		/// <returns>A result indicating success or failure.</returns>
		ValueTask<IResult> CheckAsync(CommandMeta meta, IContext context, object? value);
	}
}