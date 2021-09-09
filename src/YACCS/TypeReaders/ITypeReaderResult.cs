using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// A parsing result.
	/// </summary>
	public interface ITypeReaderResult : INestedResult
	{
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