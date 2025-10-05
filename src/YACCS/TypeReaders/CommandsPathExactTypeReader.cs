using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Trie;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses commands which start with the provided value.
/// </summary>
/// <remarks>Order is NOT guaranteed.</remarks>
public class CommandsPathExactTypeReader : CommandsTypeReader
{
	/// <inheritdoc />
	protected override IEnumerable<IImmutableCommand> GetMatchingCommands(
		IContext context,
		ICommandService commands,
		ReadOnlyMemory<string> input)
	{
		var node = commands.Commands.Root.FollowPath(input.Span);
		return node?.GetItems(recursive: false)?.Distinct() ?? [];
	}
}