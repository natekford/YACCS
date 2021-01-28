using System.Collections.Immutable;

using YACCS.Commands;

namespace YACCS.Parsing
{
	public class DefaultQuoteHandler : IQuoteHandler
	{
		private readonly IImmutableSet<char> _End;
		private readonly char _Split;
		private readonly IImmutableSet<char> _Start;

		public bool AllowEscaping { get; set; } = true;

		public DefaultQuoteHandler() : this(
			CommandServiceUtils.InternallyUsedSeparator,
			CommandServiceUtils.InternallyUsedQuotes,
			CommandServiceUtils.InternallyUsedQuotes)
		{ }

		public DefaultQuoteHandler(char split, IImmutableSet<char> start, IImmutableSet<char> end)
		{
			_Start = start;
			_End = end;
			_Split = split;
		}

		public bool ValidEndQuote(char? p, char c, char? n)
			=> ValidQuote(_End, _Start, p, c, n, allowUnionQuotes: true);

		public bool ValidSplit(char? p, char c, char? n)
			=> c == _Split && (!p.HasValue || p.Value != _Split);

		public bool ValidStartQuote(char? p, char c, char? n)
			=> ValidQuote(_Start, _End, p, c, p, allowUnionQuotes: false);

		private bool ValidQuote(
			IImmutableSet<char> quotes,
			IImmutableSet<char> otherQuotes,
			char? previous,
			char current,
			char? comparison,
			bool allowUnionQuotes)
		{
			// Current char is escaped, return false
			if (AllowEscaping && previous == '\\')
			{
				return false;
			}
			// Current char is not a quote, return false
			if (!quotes.Contains(current))
			{
				return false;
			}
			// Other comparison char doesn't exist or is whitespace, return true
			if (comparison == null || char.IsWhiteSpace(comparison.Value))
			{
				return true;
			}
			// Other comparison char is a quote char, return depending on if union quotes are allowed
			if (quotes.Contains(comparison.Value))
			{
				return allowUnionQuotes || !otherQuotes.Contains(comparison.Value);
			}
			return false;
		}
	}
}