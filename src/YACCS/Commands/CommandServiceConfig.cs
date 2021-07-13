using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands
{
	public class CommandServiceConfig
	{
		public static ImmutableCommandServiceConfig Instance { get; }
			= new CommandServiceConfig().ToImmutable();

		public HashSet<char> EndQuotes { get; set; } = new() { CommandServiceUtils.QUOTE };
		public bool IsCaseSensitive { get; set; }
		public MultiMatchHandling MultiMatchHandling { get; set; } = MultiMatchHandling.Best;
		public char Separator { get; set; } = CommandServiceUtils.SPACE;
		public HashSet<char> StartQuotes { get; set; } = new() { CommandServiceUtils.QUOTE };

		public ImmutableCommandServiceConfig ToImmutable()
			=> new(this);
	}

	public sealed class ImmutableCommandServiceConfig : ICommandServiceConfig
	{
		public IEqualityComparer<string> CommandNameComparer { get; }
		public IImmutableSet<char> EndQuotes { get; }
		public MultiMatchHandling MultiMatchHandling { get; }
		public char Separator { get; }
		public IImmutableSet<char> StartQuotes { get; }

		public ImmutableCommandServiceConfig(CommandServiceConfig mutable)
		{
			CommandNameComparer = mutable.IsCaseSensitive
				? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
			EndQuotes = mutable.EndQuotes.ToImmutableHashSet();
			MultiMatchHandling = mutable.MultiMatchHandling;
			Separator = mutable.Separator;
			StartQuotes = mutable.StartQuotes.ToImmutableHashSet();
		}

		public ImmutableCommandServiceConfig(ICommandServiceConfig other)
		{
			CommandNameComparer = other.CommandNameComparer;
			EndQuotes = other.EndQuotes;
			MultiMatchHandling = other.MultiMatchHandling;
			Separator = other.Separator;
			StartQuotes = other.StartQuotes;
		}
	}
}