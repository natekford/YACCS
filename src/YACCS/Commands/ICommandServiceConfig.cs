using System.Collections.Generic;
using System.Collections.Immutable;

namespace YACCS.Commands
{
	public interface ICommandServiceConfig
	{
		IEqualityComparer<string> CommandNameComparer { get; }
		IImmutableSet<char> EndQuotes { get; }
		bool IgnoreExtraArgs { get; }
		MultiMatchHandling MultiMatchHandling { get; }
		char Separator { get; }
		IImmutableSet<char> StartQuotes { get; }
	}
}