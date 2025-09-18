using MorseCode.ITask;

using System.Diagnostics;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <inheritdoc cref="ITypeReaderResult{T}"/>
/// <param name="innerResult">
/// <inheritdoc cref="InnerResult" path="/summary"/>
/// </param>
/// <param name="value">
/// <inheritdoc cref="Value" path="/summary"/>
/// </param>
/// <param name="successfullyParsedCount">
/// <inheritdoc cref="SuccessfullyParsedCount" path="/summary"/>
/// </param>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public partial class TypeReaderResult<T>(
	IResult innerResult,
	T? value,
	int? successfullyParsedCount
) : ITypeReaderResult<T>
{
	/// <inheritdoc />
	public IResult InnerResult { get; } = innerResult;
	/// <inheritdoc />
	public int? SuccessfullyParsedCount { get; } = successfullyParsedCount;
	/// <inheritdoc />
	public T? Value { get; } = value;
	object? ITypeReaderResult.Value => Value;
	private string DebuggerDisplay => InnerResult.IsSuccess
		? $"Value = {Value}"
		: InnerResult.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating failure.
	/// </summary>
	/// <returns>A type reader result indicating failure.</returns>
	/// <inheritdoc cref="TypeReaderResult{T}(IResult, T, int?)"/>
	public static TypeReaderResult<T> Error(IResult result)
		=> new(result, default!, null);

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating success.
	/// </summary>
	/// <returns>A type reader result indicating success.</returns>
	/// <inheritdoc cref="TypeReaderResult{T}(IResult, T, int?)"/>
	public static TypeReaderResult<T> Success(
		T value,
		int? successfullyParsedCount = null)
		=> new(Result.EmptySuccess, value, successfullyParsedCount);
}

/// <summary>
/// Cached results for <see cref="ITypeReaderResult{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class TypeReaderResult<T>
{
	/// <inheritdoc cref="Result.Canceled"/>
	public static CachedTypeReaderResult<T> Canceled { get; }
		= new(Result.Canceled);
	/// <inheritdoc cref="Result.Success"/>
	public static CachedTypeReaderResult<T> DefaultSuccess { get; }
		= new(default(T)!);
	/// <inheritdoc cref="Result.InvalidContext"/>
	public static CachedTypeReaderResult<T> InvalidContext { get; }
		= new(Result.InvalidContext);
	/// <inheritdoc cref="Result.NamedArgBadCount"/>
	public static CachedTypeReaderResult<T> NamedArgBadCount { get; }
		= new(Result.NamedArgBadCount);
	/// <inheritdoc cref="Result.NotFound" />
	public static CachedTypeReaderResult<T> NotFound { get; }
		= new(Result.NotFound(typeof(T)));
	/// <inheritdoc cref="Result.ParseFailed"/>
	public static CachedTypeReaderResult<T> ParseFailed { get; }
		= new(Result.ParseFailed(typeof(T)));
	/// <inheritdoc cref="Result.TimedOut"/>
	public static CachedTypeReaderResult<T> TimedOut { get; }
		= new(Result.TimedOut);
	/// <inheritdoc cref="Result.TooManyMatches" />
	public static CachedTypeReaderResult<T> TooManyMatches { get; }
		= new(Result.TooManyMatches(typeof(T)));
}

/// <summary>
/// Holds a result and a task returning that result.
/// </summary>
public sealed class CachedTypeReaderResult<T>
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
	/// Creates a new <see cref="CachedTypeReaderResult{T}"/>.
	/// </summary>
	/// <param name="result">The error result encountered.</param>
	public CachedTypeReaderResult(IResult result)
	{
		Result = TypeReaderResult<T>.Error(result);
		Task = Result.AsITask();
	}

	/// <summary>
	/// Creates a new <see cref="CachedTypeReaderResult{T}"/>.
	/// </summary>
	/// <param name="value">The successfully parsed value.</param>
	public CachedTypeReaderResult(T value)
	{
		Result = TypeReaderResult<T>.Success(value);
		Task = Result.AsITask();
	}
}