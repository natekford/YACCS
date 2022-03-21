using System.Diagnostics;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders;

/// <inheritdoc cref="ITypeReaderResult{T}"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public sealed class TypeReaderResult<T> : ITypeReaderResult<T>
{
	/// <inheritdoc />
	public IResult InnerResult { get; }
	/// <inheritdoc />
	public int? SuccessfullyParsedCount { get; }
	/// <inheritdoc />
	public T? Value { get; }
	object? ITypeReaderResult.Value => Value;
	private string DebuggerDisplay => InnerResult.IsSuccess
		? $"Value = {Value}"
		: InnerResult.FormatForDebuggerDisplay();

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/>.
	/// </summary>
	/// <param name="result">
	/// <inheritdoc cref="InnerResult" path="/summary"/>
	/// </param>
	/// <param name="value">
	/// <inheritdoc cref="Value" path="/summary"/>
	/// </param>
	/// <param name="successfullyParsedCount">
	/// <inheritdoc cref="SuccessfullyParsedCount" path="/summary"/>
	/// </param>
	private TypeReaderResult(
		IResult result,
		T? value,
		int? successfullyParsedCount)
	{
		InnerResult = result;
		Value = value;
		SuccessfullyParsedCount = successfullyParsedCount;
	}

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating failure.
	/// </summary>
	/// <returns>A type reader result indicating failure.</returns>
	/// <inheritdoc cref="TypeReaderResult{T}(IResult, T, int?)"/>
	public static TypeReaderResult<T> FromError(IResult result)
		=> new(result, default!, null);

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating success.
	/// </summary>
	/// <returns>A type reader result indicating success.</returns>
	/// <inheritdoc cref="TypeReaderResult{T}(IResult, T, int?)"/>
	public static TypeReaderResult<T> FromSuccess(
		T value,
		int? successfullyParsedCount = null)
		=> new(Success.Instance, value, successfullyParsedCount);
}