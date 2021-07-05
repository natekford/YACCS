using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Parsing
{
	public class ArgumentSplitter : IArgumentSplitter
	{
		private readonly IImmutableSet<char> _End;
		private readonly char _Separator;
		private readonly IImmutableSet<char> _Start;

		public static IArgumentSplitter Default { get; } = new ArgumentSplitter();
		public bool AllowEscaping { get; set; } = true;

		public ArgumentSplitter() : this(
			CommandServiceUtils.SPACE,
			CommandServiceUtils.Quotes,
			CommandServiceUtils.Quotes)
		{ }

		public ArgumentSplitter(char separator, IImmutableSet<char> start, IImmutableSet<char> end)
		{
			_Start = start;
			_End = end;
			_Separator = separator;
		}

		public string Join(ReadOnlyMemory<string> args)
		{
			if (args.Length == 0)
			{
				return string.Empty;
			}
			else if (args.Length == 1)
			{
				return args.Span[0];
			}
			return string.Join(_Separator, args.ToArray());
		}

		public bool TrySplit(ReadOnlySpan<char> input, [NotNullWhen(true)] out ReadOnlyMemory<string> args)
		{
			var result = Args.TryParse(input, this, out var temp);
			args = temp;
			return result;
		}

		public bool ValidEndQuote(char? p, char c, char? n)
			=> ValidQuote(_End, _Start, p, c, n, allowUnionQuotes: true);

		public bool ValidSplit(char? p, char c, char? n)
			=> c == _Separator && (!p.HasValue || p.Value != _Separator);

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