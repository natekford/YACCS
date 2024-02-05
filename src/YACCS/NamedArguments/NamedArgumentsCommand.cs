using YACCS.Commands.Models;

namespace YACCS.NamedArguments;

/// <summary>
/// A generated command where arguments must be passed with names.
/// </summary>
/// <inheritdoc cref="GeneratedCommand(IImmutableCommand, int)"/>
public sealed class NamedArgumentsCommand(
	IImmutableCommand source,
	int priorityDifference
) : NamedArgumentsCommand<Dictionary<string, object?>>(source, priorityDifference)
{
}