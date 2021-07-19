using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using YACCS.Commands;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class TypeReaderResult<T> : ITypeReaderResult<T>
	{
		public IResult InnerResult { get; }
		[MaybeNull]
		public T Value { get; }
		object? ITypeReaderResult.Value => Value;
		private string DebuggerDisplay => $"IsSuccess = {InnerResult.IsSuccess}, Response = {InnerResult.Response}, Value = {Value}";

		public TypeReaderResult(IResult result, [MaybeNull] T value)
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