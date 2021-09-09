using System;

namespace YACCS.Parsing
{
	/// <summary>
	/// Defines methods for joining and splitting strings.
	/// </summary>
	public interface IArgumentHandler
	{
		/// <summary>
		/// Joins <paramref name="args"/> together.
		/// </summary>
		/// <param name="args">The strings to join.</param>
		/// <returns>A string which is <paramref name="args"/> combined.</returns>
		string Join(ReadOnlyMemory<string> args);

		/// <summary>
		/// Tries to split <paramref name="input"/> into <paramref name="args"/>.
		/// </summary>
		/// <param name="input">The string to split.</param>
		/// <param name="args">The split output.</param>
		/// <returns>A bool indicating success or failure.</returns>
		/// <remarks>
		/// By default, <see cref="ArgumentHandler"/> looks for quotes when splitting
		/// and, if there are any, removes quotes from the start and end of each output
		/// string.
		/// </remarks>
		bool TrySplit(ReadOnlySpan<char> input, out ReadOnlyMemory<string> args);
	}
}