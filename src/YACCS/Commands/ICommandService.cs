using YACCS.Commands.Models;
using YACCS.Trie;

namespace YACCS.Commands;

/// <summary>
/// Defines methods for executing and searching for commands.
/// </summary>
public interface ICommandService
{
	/// <summary>
	/// The commands this command service has.
	/// </summary>
	IReadOnlyTrie<string, IImmutableCommand> Commands { get; }

	/// <summary>
	/// Executes a command.
	/// </summary>
	/// <param name="context">The context attempting to execute a command.</param>
	/// <param name="input">The input to parse a command and arguments from.</param>
	Task ExecuteAsync(IContext context, ReadOnlySpan<char> input);
}
