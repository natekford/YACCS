using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.Commands
{
	public interface ICommandService
	{
		IReadOnlyCollection<IImmutableCommand> Commands { get; }

		Task<IResult> ExecuteAsync(IContext context, string input);

		IReadOnlyList<IImmutableCommand> Find(string input);

		bool TryGetArgs(string input, [NotNullWhen(true)] out ReadOnlyMemory<string> args);
	}
}