using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;

namespace YACCS.Parsing
{
	/// <summary>
	/// Validates a start or end quote.
	/// </summary>
	/// <param name="quotes"></param>
	/// <param name="previousChar"></param>
	/// <param name="currentChar"></param>
	/// <param name="nextChar"></param>
	/// <returns></returns>
	public delegate bool ValidateQuote(IImmutableSet<char> quotes, char? previousChar, char currentChar, char? nextChar);

	/// <summary>
	/// Parses arbitrarily nested arguments from quoted strings.
	/// </summary>
	public static class Args
	{
		/// <summary>
		/// Gets either start or end indices from the supplied string using the supplied quotes.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="quotes"></param>
		/// <param name="valid"></param>
		/// <returns></returns>
		public static IReadOnlyList<int> GetIndices(
			string input,
			IImmutableSet<char> quotes,
			ValidateQuote valid)
		{
			var indices = new List<int>();
			for (var i = 0; i < input.Length; ++i)
			{
				var curr = input[i];
				var prev = i == 0 ? default(char?) : input[i - 1];
				var next = i == input.Length - 1 ? default(char?) : input[i + 1];
				if (valid(quotes, prev, curr, next))
				{
					indices.Add(i);
				}
			}
			return indices;
		}

		/// <summary>
		/// Parses args from the passed in string or throws an exception.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static List<string> Parse(string input)
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
			[NotNullWhen(true)] out List<string>? result)
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
			[NotNullWhen(true)] out List<string>? result)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				result = new List<string>();
				return true;
			}

			var startIndices = GetIndices(input, startQuotes, ValidStartQuote);
			var endIndices = GetIndices(input, endQuotes, ValidEndQuote);
			return TryParse(
				input,
				splitChar,
				startIndices,
				endIndices,
				out result
			);
		}

		/// <summary>
		/// Attempts to parse args from start and end indices.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="splitChar"></param>
		/// <param name="startIndices">Assumed to be in order.</param>
		/// <param name="endIndices">Assumed to be in order</param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(
			string input,
			char splitChar,
			IReadOnlyList<int> startIndices,
			IReadOnlyList<int> endIndices,
			[NotNullWhen(true)] out List<string>? result)
		{
			static void Add(ICollection<string> col, ReadOnlySpan<char> input)
			{
				var trimmed = input.Trim();
				if (trimmed.Length != 0)
				{
					col.Add(trimmed.ToString());
				}
			}

			static void AddRange(ICollection<string> col, ReadOnlySpan<char> input, char splitChar)
			{
				var lastStart = 0;
				for (var i = 0; i < input.Length; ++i)
				{
					if (input[i] == splitChar)
					{
						Add(col, input[lastStart..i]);
						lastStart = i;
					}
					else if (i == input.Length - 1)
					{
						Add(col, input[lastStart..^0]);
					}
				}
			}

			if (startIndices.Count != endIndices.Count)
			{
				result = null;
				return false;
			}

			var span = input.AsSpan();
			var args = new List<string>();
			// No quotes means just return splitting on space
			if (startIndices.Count == 0)
			{
				AddRange(args, span, splitChar);
				result = args;
				return true;
			}

			var minStart = startIndices[0];
			var maxEnd = endIndices[endIndices.Count - 1];
			if (minStart == 0 && maxEnd == span.Length - 1)
			{
				Add(args, span[(minStart + 1)..maxEnd]);
				result = args;
				return true;
			}

			if (minStart != 0)
			{
				AddRange(args, span[0..minStart], splitChar);
			}

			// If all start indices are less than any end index this is fairly easy
			// Just pair them off from the outside in
			if (IsPairedOffOutsideToIn(startIndices, endIndices))
			{
				Add(args, span[(startIndices[0] + 1)..endIndices[endIndices.Count - 1]]);
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
						AddRange(args, span[(previousEnd + 1)..start], splitChar);
					}

					// No starts before next end means simple quotes
					// Some starts before next end means nested quotes
					while (i < startIndices.Count - 1 && startIndices[i + 1] < end)
					{
						++i;
					}

					previousEnd = endIndices[i];
					Add(args, span[(start + 1)..previousEnd]);
				}
			}

			if (maxEnd != span.Length - 1)
			{
				AddRange(args, span[(maxEnd + 1)..^0], splitChar);
			}

			result = args;
			return true;
		}

		private static bool IsPairedOffOutsideToIn(IReadOnlyList<int> startIndices, IReadOnlyList<int> endIndices)
		{
			for (var s = 0; s < startIndices.Count; ++s)
			{
				var start = startIndices[s];
				for (var e = 0; e < endIndices.Count; ++e)
				{
					if (start > endIndices[e])
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool ValidEndQuote(IImmutableSet<char> q, char? p, char c, char? n)
			=> p != '\\' && q.Contains(c) && (n == null || char.IsWhiteSpace(n.Value) || q.Contains(n.Value));

		private static bool ValidStartQuote(IImmutableSet<char> q, char? p, char c, char? ___)
			=> p != '\\' && q.Contains(c) && (p == null || char.IsWhiteSpace(p.Value));
	}
}