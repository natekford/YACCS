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
/// <remarks>Order is NOT guaranteed</remarks>
public class CommandsPathInexactTypeReader : CommandsTypeReader
{
	/// <inheritdoc />
	protected override IEnumerable<IImmutableCommand> GetMatchingCommands(
		ICommandService commands,
		ReadOnlySpan<string> input)
	{
		var node = commands.Commands.Root.FollowPath(input);
		return node?.GetItems(recursive: true)?.Distinct() ?? [];
	}
}