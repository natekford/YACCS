using MorseCode.ITask;

using YACCS.TypeReaders;

namespace YACCS.Results;

/// <summary>
/// Cached results for <see cref="ITypeReaderResult{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class CachedResults<T>
{
	/// <inheritdoc cref="CachedResults.Canceled"/>
	public static CachedTypeReaderResult Canceled { get; }
		= new(CachedResults.Canceled);
	/// <inheritdoc cref="CachedResults.Success"/>
	public static CachedTypeReaderResult DefaultSuccess { get; }
		= new(default(T)!);
	/// <inheritdoc cref="CachedResults.InvalidContext"/>
	public static CachedTypeReaderResult InvalidContext { get; }
		= new(CachedResults.InvalidContext);
	/// <inheritdoc cref="CachedResults.NamedArgBadCount"/>
	public static CachedTypeReaderResult NamedArgBadCount { get; }
		= new(CachedResults.NamedArgBadCount);
	/// <inheritdoc cref="Results.ParseFailed"/>
	public static CachedTypeReaderResult ParseFailed { get; }
		= new(new ParseFailed(typeof(T)));
	/// <inheritdoc cref="CachedResults.TimedOut"/>
	public static CachedTypeReaderResult TimedOut { get; }
		= new(CachedResults.TimedOut);

	/// <summary>
	/// Holds a result and a task returning that result.
	/// </summary>
	public sealed class CachedTypeReaderResult
	{
		/// <summary>
		/// The result that is being cached.
		/// </summary>
		public ITypeReaderResult<T> Result { get; }
		/// <summary>
		/// A task returning <see cref="Result"/>.
		/// </summary>
		public ITask<ITypeReaderResult<T>> Task { get; }

		/// <summary>
		/// Creates a new <see cref="CachedTypeReaderResult"/>.
		/// </summary>
		/// <param name="result">The error result encountered.</param>
		public CachedTypeReaderResult(IResult result)
		{
			Result = TypeReaderResult<T>.FromError(result);
			Task = Result.AsITask();
		}

		/// <summary>
		/// Creates a new <see cref="CachedTypeReaderResult"/>.
		/// </summary>
		/// <param name="value">The successfully parsed value.</param>
		public CachedTypeReaderResult(T value)
		{
			Result = TypeReaderResult<T>.FromSuccess(value);
			Task = Result.AsITask();
		}
	}
}