using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands;

/// <summary>
/// Configuration for <see cref="CommandService"/>.
/// </summary>
/// <param name="CommandNameComparer">How to compare paths for equality.</param>
/// <param name="EndQuotes">Characters that can be used to end quotes.</param>
/// <param name="MultiMatchHandling">How to handle when multiple matching commands are found.</param>
/// <param name="Separator">Character that is used for separating arguments.</param>
/// <param name="StartQuotes">Characters that can be used to start quotes.</param>
public record CommandServiceConfig(
	IEqualityComparer<string> CommandNameComparer,
	ImmutableHashSet<char> EndQuotes,
	MultiMatchHandling MultiMatchHandling,
	char Separator,
	ImmutableHashSet<char> StartQuotes
)
{
	/// <summary>
	/// The default instance of <see cref="CommandServiceConfig"/>.
	/// </summary>
	public static CommandServiceConfig Default { get; } = new(
		CommandNameComparer: StringComparer.OrdinalIgnoreCase,
		EndQuotes: [CommandServiceUtils.QUOTE],
		MultiMatchHandling: MultiMatchHandling.Best,
		Separator: CommandServiceUtils.SPACE,
		StartQuotes: [CommandServiceUtils.QUOTE]
	);
}