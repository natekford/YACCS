using System;
using System.Collections.Generic;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses commands which have all the supplied categories.
/// </summary>
/// <remarks>Order is NOT guaranteed</remarks>
public class CommandsCategoryTypeReader : CommandsTypeReader
{
	/// <inheritdoc />
	protected override IEnumerable<IImmutableCommand> GetMatchingCommands(
		ICommandService commands,
		ReadOnlySpan<string> input)
	{
		// Create a hashset to remove duplicates and have a quicker Contains()
		var categories = new HashSet<string>(input.Length, StringComparer.OrdinalIgnoreCase);
		foreach (var category in input)
		{
			categories.Add(category);
		}

		var found = new HashSet<IImmutableCommand>();
		var matchedCategories = new HashSet<string>(categories.Count, categories.Comparer);
		foreach (var command in commands.Commands)
		{
			// Don't include generated commands because they are copies
			if (command.Source is not null)
			{
				continue;
			}

			foreach (var category in command.Categories)
			{
				if (categories.Contains(category.Category))
				{
					matchedCategories.Add(category.Category);
				}

				// An equal amount of categories found to categories searched for
				// means all have been found so we can stop looking
				if (matchedCategories.Count == categories.Count)
				{
					found.Add(command);
					break;
				}
			}
			matchedCategories.Clear();
		}

		return found;
	}
}