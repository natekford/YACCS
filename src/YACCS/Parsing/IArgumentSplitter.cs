using System;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Parsing
{
	public interface IArgumentSplitter
	{
		string Join(ReadOnlyMemory<string> args);

		bool TrySplit(ReadOnlySpan<char> input, [NotNullWhen(true)] out ReadOnlyMemory<string> args);

		bool ValidEndQuote(char? p, char c, char? n);

		bool ValidSplit(char? p, char c, char? n);

		bool ValidStartQuote(char? p, char c, char? n);
	}
}