using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses commands which have the supplied runtime id.
/// </summary>
/// <remarks>Only one command should be returned.</remarks>
public class CommandsRuntimeIdTypeReader : CommandsTypeReader
{
	/// <inheritdoc />
	protected override IEnumerable<IImmutableCommand> GetMatchingCommands(
		IContext context,
		ICommandService commands,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		return int.TryParse(joined, out var id)
			? commands.Commands.Where(x => x.RuntimeId == id).Distinct()
			: [];
	}
}