using System.Diagnostics;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <inheritdoc cref="ITypeReaderResult{T}"/>
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public sealed class TypeReaderResult<T> : ITypeReaderResult<T>
	{
		/// <inheritdoc />
		public IResult InnerResult { get; }
		/// <inheritdoc />
		public T? Value { get; }
		object? ITypeReaderResult.Value => Value;
		private string DebuggerDisplay => InnerResult.IsSuccess
			? $"Value = {Value}"
			: InnerResult.FormatForDebuggerDisplay();

		private TypeReaderResult(IResult result, T? value)
		{
			InnerResult = result;
			Value = value;
		}

		/// <summary>
		/// Creates a new <see cref="TypeReaderResult{T}"/>.
		/// </summary>
		/// <param name="result">The result to create a result for.</param>
		/// <returns>A type reader result indicating failure.</returns>
		public static TypeReaderResult<T> FromError(IResult result)
			=> new(result, default!);

		/// <summary>
		/// Creates a new <see cref="TypeReaderResult{T}"/>.
		/// </summary>
		/// <param name="value">The value to create a result for.</param>
		/// <returns>A type reader result indicating success.</returns>
		public static TypeReaderResult<T> FromSuccess(T value)
			=> new(SuccessResult.Instance, value);
	}
}