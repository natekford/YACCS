using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

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
		IReadOnlyCollection<IImmutableCommand> Commands { get; }

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="context">The context attempting to execute a command.</param>
		/// <param name="input">The input to parse a command and arguments from.</param>
		/// <returns>A failure result or <see cref="SuccessResult.Instance"/>.</returns>
		/// <inheritdoc cref="IExecuteResult" path="/remarks"/>
		ValueTask<IExecuteResult> ExecuteAsync(IContext context, string input);

		/// <summary>
		/// Finds a command via <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The path to search for.</param>
		/// <returns>A collection of matching commands.</returns>
		IReadOnlyCollection<IImmutableCommand> FindByPath(ReadOnlyMemory<string> input);
	}
}