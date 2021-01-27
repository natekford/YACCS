using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Parsing
{
	/// <summary>
	/// Parses arbitrarily nested arguments from quoted strings.
	/// </summary>
	public static class Args
	{
		/// <summary>
		/// Parses args from the passed in string or throws an exception.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string[] Parse(string input)
		{
			if (TryParse(input, out var result))
			{
				return result;
			}
			throw new ArgumentException("There is a quote mismatch.");
		}

		/// <summary>
		/// Attempts to parse args using the default quote character " for beginning and ending quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			[NotNullWhen(true)] out string[]? result)
		{
			return TryParse(
				input,
				CommandServiceUtils.InternallyUsedSeparator,
				CommandServiceUtils.InternallyUsedQuotes,
				CommandServiceUtils.InternallyUsedQuotes,
				out result
			);
		}

		/// <summary>
		/// Attempts to parse args from characters indicating the start of a quote and characters indicating the end of a quote.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="splitChar"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			char splitChar,
			IImmutableSet<char> startQuotes,
			IImmutableSet<char> endQuotes,
			[NotNullWhen(true)] out string[]? result)
		{
			static void Add(string[] col, ref int index, ReadOnlySpan<char> input)
			{
				var trimmed = input.Trim();
				if (trimmed.Length != 0)
				{
					col[index] = trimmed.ToString();
					++index;
				}
			}

			static void AddRange(string[] col, ref int index, ReadOnlySpan<char> input, char splitChar)
			{
				var lastStart = 0;
				for (var i = 0; i < input.Length; ++i)
				{
					if (input[i] == splitChar)
					{
						Add(col, ref index, input[lastStart..i]);
						lastStart = i;
					}
					else if (i == input.Length - 1)
					{
						Add(col, ref index, input[lastStart..^0]);
					}
				}
			}

			if (string.IsNullOrWhiteSpace(input))
			{
				result = Array.Empty<string>();
				return true;
			}

			var info = GetQuoteInfo(input, splitChar, startQuotes, endQuotes);

			// Quote mismatch, indicate error
			if (info.CurrentDepth != 0)
			{
				result = null;
				return false;
			}

			var span = input.AsSpan();
			var index = 0;
			result = new string[info.Size];

			// No quotes in string, split everything
			if (info.MinStart == QuoteInfo.DEFAULT)
			{
				AddRange(result, ref index, span, splitChar);
				return true;
			}

			// Entire string is quoted, split nothing
			if (info.MinStart == 0 && info.MaxEnd == span.Length - 1)
			{
				Add(result, ref index, span[(info.MinStart + 1)..info.MaxEnd]);
				return true;
			}

			// Quotes start mid way through the string, add the first unquoted bit
			if (info.MinStart != 0)
			{
				AddRange(result, ref index, span[0..info.MinStart], splitChar);
			}

			// All start indices are less than end indices indicates something similar to
			// the entire string being quoted, in that everything between those 2 extrema
			// can be treated as a single quoted string
			if (info.MaxStart < info.MinEnd)
			{
				Add(result, ref index, span[(info.MinStart + 1)..info.MaxEnd]);
			}
			else
			{
				int start = info.MinStart, end = info.MinEnd, previousEnd = int.MaxValue;
				do
				{
					// Keep track of the start for this iteration
					var iStart = start;

					// If the previous end is before the current start that means everything
					// in between is ignored unless we manually add it, so do that
					if (previousEnd < start)
					{
						AddRange(result, ref index, span[(previousEnd + 1)..start], splitChar);
					}

					// No starts before next end means simple quotes
					// Some starts before next end means nested quotes
					var nestedCount = 0;
					// Begin with incrementing start since we already know well enough to
					// skip the current index
					for (++start; start < end; ++start)
					{
						var (prev, curr, next) = input.GetChars(start);
						if (ValidStartQuote(startQuotes, prev, curr, next))
						{
							++nestedCount;
						}
					}
					// Begin with incrementing end due to the reason explained above
					for (++end; end <= info.MaxEnd && nestedCount > 0; ++end)
					{
						var (prev, curr, next) = input.GetChars(end);
						if (ValidEndQuote(endQuotes, prev, curr, next))
						{
							--nestedCount;
						}
					}

					// Subtract 1 to account for removing the quote itself
					previousEnd = end - 1;
					Add(result, ref index, span[(iStart + 1)..previousEnd]);

					// Iterate to the next start/end quotes
					for (; start < info.MaxStart; ++start)
					{
						var (prev, curr, next) = input.GetChars(start);
						if (ValidStartQuote(startQuotes, prev, curr, next))
						{
							break;
						}
					}
					for (; end < info.MaxEnd; ++end)
					{
						var (prev, curr, next) = input.GetChars(end);
						if (ValidEndQuote(endQuotes, prev, curr, next))
						{
							break;
						}
					}
				} while (start <= info.MaxStart);
			}

			// Quotes stop mid way through the string, add the last unquoted bit
			if (info.MaxEnd != span.Length - 1)
			{
				AddRange(result, ref index, span[(info.MaxEnd + 1)..^0], splitChar);
			}

			return true;
		}

		private static (char? Prev, char Curr, char? Next) GetChars(this string input, int i)
		{
			var prev = i == 0 ? default(char?) : input[i - 1];
			var curr = input[i];
			var next = i == input.Length - 1 ? default(char?) : input[i + 1];
			return (prev, curr, next);
		}

		private static QuoteInfo GetQuoteInfo(
			string input,
			char splitChar,
			IImmutableSet<char> startQuotes,
			IImmutableSet<char> endQuotes)
		{
			const int DEFAULT = QuoteInfo.DEFAULT;

			int maxDepth = 0, currentDepth = 0, size = 1,
				minStart = DEFAULT, maxStart = DEFAULT, startCount = 0,
				minEnd = DEFAULT, maxEnd = DEFAULT, endCount = 0;
			for (var i = 0; i < input.Length; ++i)
			{
				var (prev, curr, next) = input.GetChars(i);
				if (currentDepth == 0 && curr == splitChar)
				{
					++size;
				}
				else if (ValidStartQuote(startQuotes, prev, curr, next))
				{
					++currentDepth;
					maxDepth = Math.Max(maxDepth, currentDepth);

					++startCount;
					maxStart = i;
					if (minStart == DEFAULT)
					{
						minStart = i;
					}
				}
				else if (ValidEndQuote(endQuotes, prev, curr, next))
				{
					--currentDepth;

					++endCount;
					maxEnd = i;
					if (minEnd == DEFAULT)
					{
						minEnd = i;
					}
				}
			}
			return new QuoteInfo(
				currentDepth, endCount, maxDepth, maxEnd, maxStart,
				minEnd, minStart, size, startCount);
		}

		private static bool ValidEndQuote(IImmutableSet<char> q, char? p, char c, char? n)
			=> p != '\\' && q.Contains(c) && (n == null || char.IsWhiteSpace(n.Value) || q.Contains(n.Value));

		private static bool ValidStartQuote(IImmutableSet<char> q, char? p, char c, char? _)
			=> p != '\\' && q.Contains(c) && (p == null || char.IsWhiteSpace(p.Value));

		private readonly struct QuoteInfo
		{
			public const int DEFAULT = -1;

			public int CurrentDepth { get; }
			public int EndCount { get; }
			public int MaxDepth { get; }
			public int MaxEnd { get; }
			public int MaxStart { get; }
			public int MinEnd { get; }
			public int MinStart { get; }
			public int Size { get; }
			public int StartCount { get; }

			public QuoteInfo(
				int currentDepth, int endCount, int maxDepth, int maxEnd, int maxStart,
				int minEnd, int minStart, int size, int startCount)
			{
				CurrentDepth = currentDepth;
				EndCount = endCount;
				MaxDepth = maxDepth;
				MaxEnd = maxEnd;
				MaxStart = maxStart;
				MinEnd = minEnd;
				MinStart = minStart;
				Size = size;
				StartCount = startCount;
			}
		}
	}
}