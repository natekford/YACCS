using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.Trie;

namespace YACCS.Commands
{
	/// <summary>
	/// Defines methods for executing and searching for commands.
	/// </summary>
	public interface ICommandService
	{
		/// <summary>
		/// The commands this command service has.
		/// </summary>
		ITrie<string, IImmutableCommand> Commands { get; }

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="context">The context attempting to execute a command.</param>
		/// <param name="input">The input to parse a command and arguments from.</param>
		/// <returns>A failure result or <see cref="SuccessResult.Instance"/>.</returns>
		/// <inheritdoc cref="IExecuteResult" path="/remarks"/>
		ValueTask<IExecuteResult> ExecuteAsync(IContext context, string input);
	}
}