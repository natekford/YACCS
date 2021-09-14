using System.Collections.Immutable;

namespace YACCS.Commands
{
	/// <summary>
	/// Configuration for <see cref="CommandService"/>.
	/// </summary>
	public interface ICommandServiceConfig
	{
		/// <summary>
		/// How to compare paths for equality.
		/// </summary>
		IEqualityComparer<string> CommandNameComparer { get; }
		/// <summary>
		/// Characters that can be used to end quotes.
		/// </summary>
		IImmutableSet<char> EndQuotes { get; }
		/// <summary>
		/// How to handle
		/// </summary>
		MultiMatchHandling MultiMatchHandling { get; }
		/// <summary>
		/// Character that is used for separating arguments.
		/// </summary>
		char Separator { get; }
		/// <summary>
		/// Characters that can be used to start quotes.
		/// </summary>
		IImmutableSet<char> StartQuotes { get; }
	}
}