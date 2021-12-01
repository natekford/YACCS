using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// A parsing result.
	/// </summary>
	public interface ITypeReaderResult : INestedResult
	{
		/// <summary>
		/// The amount of strings which were successfully parsed.
		/// </summary>
		/// <remarks>
		/// In a successful case, <see langword="null"/> indicates to ignore this property
		/// and advance the argument parsing to after the passed in strings.
		/// In a failed case, this property's value has no affect.
		/// </remarks>
		int? SuccessfullyParsedCount { get; }
		/// <summary>
		/// The value of this result.
		/// </summary>
		/// <remarks>
		/// <see langword="null"/> does not necessarily mean failure, check
		/// <see cref="INestedResult.InnerResult"/> instead.
		/// </remarks>
		object? Value { get; }
	}
}