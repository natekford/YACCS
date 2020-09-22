using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace YACCS.Parsing
{
	/// <summary>
	/// Parses arbitrarily nested arguments from quoted strings.
	/// </summary>
	public readonly struct ParseArgs
	{
		private static readonly ImmutableArray<char> _DefautQuotes = new[] { '"' }.ToImmutableArray();
		private static readonly char[] _SplitChars = new[] { ' ' };

		/// <summary>
		/// The parsed arguments.
		/// </summary>
		public IReadOnlyList<string> Arguments { get; }
		/// <summary>
		/// The characters used to end quotes with.
		/// </summary>
		public IReadOnlyList<char> EndingQuoteCharacters { get; }
		/// <summary>
		/// The characters used to start quotes with.
		/// </summary>
		public IReadOnlyList<char> StartingQuoteCharacters { get; }

		/// <summary>
		/// Creates an instance of <see cref="ParseArgs"/>.
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="startingQuotes"></param>
		/// <param name="endingQuotes"></param>
		public ParseArgs(
			IEnumerable<string> arguments,
			IEnumerable<char> startingQuotes,
			IEnumerable<char> endingQuotes)
			: this(arguments.ToImmutableArray(), startingQuotes.ToImmutableArray(), endingQuotes.ToImmutableArray())
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="ParseArgs"/>.
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="startingQuotes"></param>
		/// <param name="endingQuotes"></param>
		public ParseArgs(
			IReadOnlyList<string> arguments,
			IReadOnlyList<char> startingQuotes,
			IReadOnlyList<char> endingQuotes)
		{
			Arguments = arguments;
			StartingQuoteCharacters = startingQuotes;
			EndingQuoteCharacters = endingQuotes;
		}

		/// <summary>
		/// Gets either start or end indexes from the supplied string using the supplied quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="quotes"></param>
		/// <param name="valid"></param>
		/// <returns></returns>
		public static IReadOnlyList<int> GetIndices(
			string input,
			IReadOnlyList<char> quotes,
			ValidateQuote valid)
		{
			var indexes = new List<int>();
			for (var i = 0; i < input.Length; ++i)
			{
				var curr = input[i];
				if (!quotes.Contains(curr))
				{
					continue;
				}

				var prev = i == 0 ? default(char?) : input[i - 1];
				var next = i == input.Length - 1 ? default(char?) : input[i + 1];
				if (valid(prev, curr, next))
				{
					indexes.Add(i);
				}
			}
			return indexes;
		}

		/// <summary>
		/// Parses a <see cref="ParseArgs"/> from the passed in string or throws an exception.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static ParseArgs Parse(string input)
		{
			if (TryParse(input, out var result))
			{
				return result;
			}
			throw new ArgumentException("There is a quote mismatch.");
		}

		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> using the default quote character " for beginning and ending quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(string input, out ParseArgs result)
			=> TryParse(input, _DefautQuotes, _DefautQuotes, out result);

		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> from characters indicating the start of a quote and characters indicating the end of a quote.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			IReadOnlyList<char> startQuotes,
			IReadOnlyList<char> endQuotes,
			out ParseArgs result)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				result = new ParseArgs(Enumerable.Empty<string>(), startQuotes, endQuotes);
				return true;
			}

			var startIndexes = GetIndices(input, startQuotes,
				(p, _, _) => p != '\\' && (p == null || char.IsWhiteSpace(p.Value)));
			var endIndexes = GetIndices(input, endQuotes,
				(p, _, n) => p != '\\' && (n == null || char.IsWhiteSpace(n.Value) || endQuotes.Contains(n.Value)));
			return TryParse(input, startQuotes, endQuotes, startIndexes, endIndexes, out result);
		}

		/// <summary>
		/// Attempts to parse a <see cref="ParseArgs"/> from start and end indexes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startQuotes"></param>
		/// <param name="endQuotes"></param>
		/// <param name="startIndices">Assumed to be in order.</param>
		/// <param name="endIndices">Assumed to be in order</param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			IReadOnlyList<char> startQuotes,
			IReadOnlyList<char> endQuotes,
			IReadOnlyList<int> startIndices,
			IReadOnlyList<int> endIndices,
			out ParseArgs result)
		{
			if (startIndices.Count != endIndices.Count)
			{
				result = default;
				return false;
			}
			// No quotes means just return splitting on space
			if (startIndices.Count == 0)
			{
				var immutable = input.Split(' ').ToImmutableArray();
				result = new ParseArgs(immutable, startQuotes, endQuotes);
				return true;
			}

			var minStart = startIndices[0];
			var maxEnd = endIndices[endIndices.Count - 1];
			if (minStart == 0 && maxEnd == input.Length - 1)
			{
				var trimmed = GetTrimmedString(input, minStart + 1, maxEnd);
				var immutable = new[] { trimmed }.ToImmutableArray();
				result = new ParseArgs(immutable, startQuotes, endQuotes);
				return true;
			}

			var args = ImmutableArray.CreateBuilder<string>();
			if (minStart != 0)
			{
				args.AddRange(Split(input.Substring(0, minStart)));
			}
			// If all start indexes are less than any end index this is fairly easy
			// Just pair them off from the outside in
			if (!startIndices.Any(s => endIndices.Any(e => s > e)))
			{
				AddIfNotWhitespace(args, input, startIndices[0], endIndices[endIndices.Count - 1] + 1);
			}
			else
			{
				for (int i = 0, previousEnd = int.MaxValue; i < startIndices.Count; ++i)
				{
					var start = startIndices[i];
					var end = endIndices[i];

					// If the last ending is before the current start that means everything
					// in between is ignored unless we manually add it + we need to split it
					if (previousEnd < start)
					{
						var diff = start - previousEnd - 1;
						args.AddRange(Split(input.Substring(previousEnd + 1, diff)));
					}

					// No starts before next end means simple quotes
					// Some starts before next end means nested quotes
					var skip = startIndices.Skip(i + 1).Count(x => x < end);
					previousEnd = endIndices[i += skip] + 1;
					AddIfNotWhitespace(args, input, start, previousEnd);
				}
			}

			if (maxEnd != input.Length - 1)
			{
				args.AddRange(Split(input.Substring(maxEnd + 1)));
			}

			result = new ParseArgs(args.MoveToImmutable(), startQuotes, endQuotes);
			return true;
		}

		private static void AddIfNotWhitespace(
			ImmutableArray<string>.Builder builder,
			string input,
			int startIndex,
			int endIndex)
		{
			var trimmed = GetTrimmedString(input, startIndex, endIndex);
			if (trimmed.Length != 0)
			{
				builder.Add(trimmed);
			}
		}

		private static string GetTrimmedString(string input, int startIndex, int endIndex)
			=> input[startIndex..endIndex].Trim();

		private static string[] Split(string input)
			=> input.Split(_SplitChars, StringSplitOptions.RemoveEmptyEntries);

		/// <summary>
		/// Validates a start or end quote.
		/// </summary>
		/// <param name="previousChar"></param>
		/// <param name="currentChar"></param>
		/// <param name="nextChar"></param>
		/// <returns></returns>
		public delegate bool ValidateQuote(char? previousChar, char currentChar, char? nextChar);
	}
}