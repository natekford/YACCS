using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<IImmutableCommand> Commands { get; }

		ValueTask<ICommandResult> ExecuteAsync(IContext context, string input);

		IReadOnlyList<IImmutableCommand> Find(ReadOnlyMemory<string> input);
	}
}