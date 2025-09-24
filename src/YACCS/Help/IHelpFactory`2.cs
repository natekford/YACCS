using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Help;

/// <summary>
/// Converts a singular command to a <typeparamref name="TOutput"/>.
/// </summary>
public interface IHelpFactory<TContext, TOutput>
	where TContext : IContext
{
	/// <summary>
	/// Formats <paramref name="command"/> as a <typeparamref name="TOutput"/> for a help command.
	/// </summary>
	/// <param name="context">The context invoking this help method.</param>
	/// <param name="command">The command to format.</param>
	/// <returns>A <typeparamref name="TOutput"/> representing <paramref name="command"/>.</returns>
	ValueTask<TOutput> CreateHelpAsync(TContext context, IImmutableCommand command);
}