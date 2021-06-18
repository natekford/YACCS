using System;
using System.Diagnostics.CodeAnalysis;

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
		public static string[] Parse(ReadOnlySpan<char> input)
		{
			if (TryParse(input, out var result))
			{
				return result;
			}
			throw new QuoteMismatchException("There is a quote mismatch.", nameof(input));
		}

		/// <summary>
		/// Attempts to parse args using the default quote character " for beginning and ending quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			ReadOnlySpan<char> input,
			[NotNullWhen(true)] out string[]? result)
			=> TryParse(input, ArgumentSplitter.Default, out result);

		/// <summary>
		/// Attempts to parse args from characters indicating the start of a quote and characters indicating the end of a quote.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="splitter"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			ReadOnlySpan<char> input,
			IArgumentSplitter splitter,
			[NotNullWhen(true)] out string[]? result)
		{
			static void Add(string[] col, ref int index, ReadOnlySpan<char> input)
				=> col[index++] = input.Trim().ToString();

			static void AddRange(string[] col, ref int index, ReadOnlySpan<char> input, IArgumentSplitter splitter)
			{
				input = input.Trim();
				if (input.Length == 0)
				{
					return;
				}

				var lastStart = 0;
				for (var i = 0; i < input.Length; ++i)
				{
					var (prev, curr, next) = input.GetChars(i);
					if (splitter.ValidSplit(prev, curr, next))
					{
						Add(col, ref index, input[lastStart..i]);
						lastStart = i + 1;
					}
					else if (i == input.Length - 1)
					{
						Add(col, ref index, input[lastStart..^0]);
					}
				}
			}

			if (input.IsEmpty || input.IsWhiteSpace())
			{
				result = Array.Empty<string>();
				return true;
			}

			var info = QuoteInfo.Create(input, splitter);

			// Quote mismatch, indicate error
			if (info.CurrentDepth != 0)
			{
				result = null;
				return false;
			}

			var index = 0;
			result = new string[info.Size];

			// No quotes in string, split everything
			if (info.HasNoQuotes)
			{
				AddRange(result, ref index, input, splitter);
				return true;
			}

			// Entire string is quoted, split nothing
			if (info.MinStart == 0 && info.MaxEnd == input.Length - 1)
			{
				Add(result, ref index, input[(info.MinStart + 1)..info.MaxEnd]);
				return true;
			}

			// Quotes start mid way through the string, add the first unquoted bit
			if (info.MinStart != 0)
			{
				AddRange(result, ref index, input[0..info.MinStart], splitter);
			}

			// All start indices are less than end indices indicates something similar to
			// the entire string being quoted, in that everything between those 2 extrema
			// can be treated as a single quoted string
			if (info.MaxStart < info.MinEnd)
			{
				Add(result, ref index, input[(info.MinStart + 1)..info.MaxEnd]);
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
						AddRange(result, ref index, input[(previousEnd + 1)..start], splitter);
					}

					// No starts before next end means simple quotes
					// Some starts before next end means nested quotes
					var nestedCount = 0;
					// Begin with incrementing start since we already know well enough to
					// skip the current index
					for (++start; start < end; ++start)
					{
						if (splitter.ValidStartQuote(input, start))
						{
							++nestedCount;
						}
					}
					// Begin with incrementing end due to the reason explained above
					for (++end; end <= info.MaxEnd && nestedCount > 0; ++end)
					{
						if (splitter.ValidEndQuote(input, end))
						{
							--nestedCount;
						}
					}

					// If end is somehow less than start, there is a quote mismatch somewhere
					// Best we can do is return false with the partially filled array
					if (end <= iStart)
					{
						return false;
					}

					// Subtract 1 to account for removing the quote itself
					previousEnd = end - 1;
					Add(result, ref index, input[(iStart + 1)..previousEnd]);

					// Iterate to the next start/end quotes
					while (start < info.MaxStart && !splitter.ValidStartQuote(input, start))
					{
						++start;
					}
					while (end < info.MaxEnd && !splitter.ValidEndQuote(input, end))
					{
						++end;
					}
				} while (start <= info.MaxStart);
			}

			// Quotes stop mid way through the string, add the last unquoted bit
			if (info.MaxEnd != input.Length - 1)
			{
				AddRange(result, ref index, input[(info.MaxEnd + 1)..^0], splitter);
			}

			return true;
		}

		private static (char? Prev, char Curr, char? Next) GetChars(this ReadOnlySpan<char> input, int i)
		{
			var prev = i == 0 ? default(char?) : input[i - 1];
			var curr = input[i];
			var next = i == input.Length - 1 ? default(char?) : input[i + 1];
			return (prev, curr, next);
		}

		private static bool ValidEndQuote(this IArgumentSplitter splitter, ReadOnlySpan<char> input, int i)
		{
			var (prev, curr, next) = input.GetChars(i);
			return splitter.ValidEndQuote(prev, curr, next);
		}

		private static bool ValidStartQuote(this IArgumentSplitter splitter, ReadOnlySpan<char> input, int i)
		{
			var (prev, curr, next) = input.GetChars(i);
			return splitter.ValidStartQuote(prev, curr, next);
		}

		private readonly struct QuoteInfo
		{
			private const int DEFAULT = -1;

			public int CurrentDepth { get; }
			public int EndCount { get; }
			public bool HasNoQuotes => MinStart == DEFAULT;
			public int MaxDepth { get; }
			public int MaxEnd { get; }
			public int MaxStart { get; }
			public int MinEnd { get; }
			public int MinStart { get; }
			public int Size { get; }
			public int StartCount { get; }

			private QuoteInfo(
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

			public static QuoteInfo Create(ReadOnlySpan<char> input, IArgumentSplitter splitter)
			{
				int maxDepth = 0, currentDepth = 0, size = 1,
					minStart = DEFAULT, maxStart = DEFAULT, startCount = 0,
					minEnd = DEFAULT, maxEnd = DEFAULT, endCount = 0;
				for (var i = 0; i < input.Length; ++i)
				{
					var (prev, curr, next) = input.GetChars(i);
					if (currentDepth == 0 && splitter.ValidSplit(prev, curr, next))
					{
						++size;
						continue;
					}

					// Don't use else in this since a quote can technically be both start and end
					// and we want to return a failure instead of throwing ArgumentOutOfRange
					if (splitter.ValidStartQuote(prev, curr, next))
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
					if (splitter.ValidEndQuote(prev, curr, next))
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
		}
	}
}