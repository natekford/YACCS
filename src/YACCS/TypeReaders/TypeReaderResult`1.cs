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

	private TypeReaderResult(IResult result, T? value, int? successfullyParsedCount)
	{
		InnerResult = result;
		Value = value;
		SuccessfullyParsedCount = successfullyParsedCount;
	}

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating failure.
	/// </summary>
	/// <param name="result">The result to create a result for.</param>
	/// <returns>A type reader result indicating failure.</returns>
	public static TypeReaderResult<T> FromError(IResult result)
		=> new(result, default!, null);

	/// <summary>
	/// Creates a new <see cref="TypeReaderResult{T}"/> indicating success.
	/// </summary>
	/// <param name="value">The value to create a result for.</param>
	/// <param name="successfullyParsedCount">
	/// The amount of strings which were successfully parsed.
	/// </param>
	/// <returns>A type reader result indicating success.</returns>
	public static TypeReaderResult<T> FromSuccess(T value, int? successfullyParsedCount = null)
		=> new(SuccessResult.Instance, value, successfullyParsedCount);
}