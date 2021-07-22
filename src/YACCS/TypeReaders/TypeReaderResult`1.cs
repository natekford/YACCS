using System.Diagnostics;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class TypeReaderResult<T> : ITypeReaderResult<T>
	{
		public IResult InnerResult { get; }
		public T? Value { get; }
		object? ITypeReaderResult.Value => Value;
		private string DebuggerDisplay => InnerResult.IsSuccess
			? $"Value = {Value}"
			: InnerResult.FormatForDebuggerDisplay();

		public TypeReaderResult(IResult result, T? value)
		{
			InnerResult = result;
			Value = value;
		}

		public static TypeReaderResult<T> FromError(IResult result)
			=> new(result, default!);

		public static TypeReaderResult<T> FromSuccess(T value)
			=> new(SuccessResult.Instance, value);
	}
}