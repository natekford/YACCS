using System.Collections.Immutable;

namespace YACCS.Commands;

/// <inheritdoc cref="ICommandServiceConfig" />
public class CommandServiceConfig
{
	/// <summary>
	/// A singleton instance of <see cref="ICommandServiceConfig"/>.
	/// </summary>
	public static ICommandServiceConfig Instance { get; }
		= new CommandServiceConfig().ToImmutable();

	/// <inheritdoc cref="ICommandServiceConfig.EndQuotes" />
	public HashSet<char> EndQuotes { get; set; } = new() { CommandServiceUtils.QUOTE };
	/// <summary>
	/// Whether or not commands are case sensitive.
	/// </summary>
	public bool IsCaseSensitive { get; set; }
	/// <inheritdoc cref="ICommandServiceConfig.MultiMatchHandling" />
	public MultiMatchHandling MultiMatchHandling { get; set; } = MultiMatchHandling.Best;
	/// <inheritdoc cref="ICommandServiceConfig.Separator" />
	public char Separator { get; set; } = CommandServiceUtils.SPACE;
	/// <inheritdoc cref="ICommandServiceConfig.StartQuotes" />
	public HashSet<char> StartQuotes { get; set; } = new() { CommandServiceUtils.QUOTE };

	/// <summary>
	/// Creates a new immutable version of this config.
	/// </summary>
	/// <returns></returns>
	public ICommandServiceConfig ToImmutable()
		=> new ImmutableCommandServiceConfig(this);

	private sealed class ImmutableCommandServiceConfig : ICommandServiceConfig
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
