
using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Help;

/// <summary>
/// Defines a method for formatting a command to a string.
/// </summary>
public interface IHelpFormatter
{
	/// <summary>
	/// Formats <paramref name="command"/> as a string for a help command.
	/// </summary>
	/// <param name="context">The context invoking this help method.</param>
	/// <param name="command">The command to format.</param>
	/// <returns>A string representing <paramref name="command"/>.</returns>
	ValueTask<string> FormatAsync(IContext context, IImmutableCommand command);
}
