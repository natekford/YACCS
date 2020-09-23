using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands
{
	public class CommandServiceConfig : ICommandServiceConfig
	{
		public static ICommandServiceConfig Default { get; }
			= new ImmutableCommandServiceConfig(new CommandServiceConfig());

		public IEqualityComparer<string> CommandNameComparer
			=> IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
		public HashSet<char> EndQuotes { get; set; } = new HashSet<char>();
		public bool IgnoreExtraArgs { get; set; }
		public bool IsCaseSensitive { get; set; }
		public MultiMatchHandling MultiMatchHandling { get; set; } = MultiMatchHandling.Best;
		public HashSet<char> Separators { get; set; } = new HashSet<char> { ' ' };
		public HashSet<char> StartQuotes { get; set; } = new HashSet<char>();
		IImmutableSet<char> ICommandServiceConfig.EndQuotes => EndQuotes.ToImmutableHashSet();
		IImmutableSet<char> ICommandServiceConfig.Separators => Separators.ToImmutableHashSet();
		IImmutableSet<char> ICommandServiceConfig.StartQuotes => StartQuotes.ToImmutableHashSet();

		private sealed class ImmutableCommandServiceConfig : ICommandServiceConfig
		{
			public IEqualityComparer<string> CommandNameComparer { get; }
			public IImmutableSet<char> EndQuotes { get; }
			public bool IgnoreExtraArgs { get; }
			public MultiMatchHandling MultiMatchHandling { get; }
			public IImmutableSet<char> Separators { get; }
			public IImmutableSet<char> StartQuotes { get; }

			public ImmutableCommandServiceConfig(ICommandServiceConfig other)
			{
				CommandNameComparer = other.CommandNameComparer;
				EndQuotes = other.EndQuotes;
				IgnoreExtraArgs = other.IgnoreExtraArgs;
				MultiMatchHandling = other.MultiMatchHandling;
				Separators = other.Separators;
				StartQuotes = other.StartQuotes;
			}
		}
	}
}