using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Parsing
{
	/// <summary>
	/// Handles joining and splitting strings.
	/// </summary>
	public sealed class ArgumentHandler : IArgumentHandler
	{
		private readonly IImmutableSet<char> _End;
		private readonly char _Separator;
		private readonly IImmutableSet<char> _Start;

		/// <summary>
		/// Whether or not quotes are allowed to be escaped.
		/// </summary>
		public bool AllowEscaping { get; set; } = true;

		/// <summary>
		/// Creates a new <see cref="ArgumentHandler"/>.
		/// </summary>
		/// <param name="separator">The character to treat as a space.</param>
		/// <param name="start">The characters to treat as the start of a quote.</param>
		/// <param name="end">The characters to treat as the end of a quote.</param>
		/// <remarks>
		/// To prevent needing to backtrack, <paramref name="start"/> and <paramref name="end"/>
		/// do not have to be the same type of quote to count as a single quote.
		/// <br/>
		/// <br/>
		/// Both "blah blah" and «blah blah" will produce the output 'blah blah'
		/// </remarks>
		public ArgumentHandler(char separator, IImmutableSet<char> start, IImmutableSet<char> end)
		{
			_Start = start;
			_End = end;
			_Separator = separator;
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public bool TrySplit(ReadOnlySpan<char> input, out ReadOnlyMemory<string> args)
		{
			var result = TryParse(input, out var temp);
			args = temp;
			return result;
		}

		private static (char? Prev, char Curr, char? Next) GetChars(ReadOnlySpan<char> input, int i)
		{
			var prev = i == 0 ? default(char?) : input[i - 1];
			var curr = input[i];
			var next = i == input.Length - 1 ? default(char?) : input[i + 1];
			return (prev, curr, next);
		}

		private void Add(string[] col, ref int index, ReadOnlySpan<char> input)
			=> col[index++] = input.Trim().ToString();

		private void AddRange(string[] col, ref int index, ReadOnlySpan<char> input)
		{
			input = input.Trim();
			if (input.Length == 0)
			{
				return;
			}

			var lastStart = 0;
			for (var i = 0; i < input.Length; ++i)
			{
				var (prev, curr, next) = GetChars(input, i);
				if (ValidSplit(prev, curr))
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

		private QuoteInfo CreateQuoteInfo(ReadOnlySpan<char> input)
		{
			const int DEFAULT = QuoteInfo.DEFAULT;

			int maxDepth = 0, currentDepth = 0, size = 1,
				minStart = DEFAULT, maxStart = DEFAULT, startCount = 0,
				minEnd = DEFAULT, maxEnd = DEFAULT, endCount = 0;
			for (var i = 0; i < input.Length; ++i)
			{
				var (prev, curr, next) = GetChars(input, i);
				if (currentDepth == 0 && ValidSplit(prev, curr))
				{
					++size;
					continue;
				}

				// Don't use else in this since a quote can technically be both start and end
				// and we want to return a failure instead of throwing ArgumentOutOfRange
				if (ValidStartQuote(prev, curr))
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
				else if (ValidEndQuote(prev, curr, next))
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
			return new(
				currentDepth, endCount, maxDepth, maxEnd, maxStart,
				minEnd, minStart, size, startCount);
		}

		private bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out string[]? result)
		{
			if (input.IsEmpty || input.IsWhiteSpace())
			{
				result = Array.Empty<string>();
				return true;
			}

			var info = CreateQuoteInfo(input);

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
				AddRange(result, ref index, input);
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
				AddRange(result, ref index, input[0..info.MinStart]);
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
						AddRange(result, ref index, input[(previousEnd + 1)..start]);
					}

					// No starts before next end means simple quotes
					// Some starts before next end means nested quotes
					var nestedCount = 0;
					// Begin with incrementing start since we already know well enough to
					// skip the current index
					for (++start; start < end; ++start)
					{
						if (ValidStartQuote(input, start))
						{
							++nestedCount;
						}
					}
					// Begin with incrementing end due to the reason explained above
					for (++end; end <= info.MaxEnd && nestedCount > 0; ++end)
					{
						if (ValidEndQuote(input, end))
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
					while (start < info.MaxStart && !ValidStartQuote(input, start))
					{
						++start;
					}
					while (end < info.MaxEnd && !ValidEndQuote(input, end))
					{
						++end;
					}
				} while (start <= info.MaxStart);
			}

			// Quotes stop mid way through the string, add the last unquoted bit
			if (info.MaxEnd != input.Length - 1)
			{
				AddRange(result, ref index, input[(info.MaxEnd + 1)..^0]);
			}

			return true;
		}

		private bool ValidEndQuote(ReadOnlySpan<char> input, int i)
		{
			var (prev, curr, next) = GetChars(input, i);
			return ValidEndQuote(prev, curr, next);
		}

		private bool ValidEndQuote(char? prev, char curr, char? next)
			=> ValidQuote(_End, _Start, prev, curr, next, allowUnionQuotes: true);

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
			if (comparison is null || char.IsWhiteSpace(comparison.Value))
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

		private bool ValidSplit(char? prev, char curr)
			=> curr == _Separator && (!prev.HasValue || prev.Value != _Separator);

		private bool ValidStartQuote(ReadOnlySpan<char> input, int i)
		{
			var (prev, curr, _) = GetChars(input, i);
			return ValidStartQuote(prev, curr);
		}

		private bool ValidStartQuote(char? prev, char curr)
			=> ValidQuote(_Start, _End, prev, curr, prev, allowUnionQuotes: false);

		private readonly struct QuoteInfo
		{
			public const int DEFAULT = -1;

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