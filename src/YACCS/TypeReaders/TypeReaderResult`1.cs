using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using YACCS.Results;

namespace YACCS.TypeReaders
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class TypeReaderResult<T> : ITypeReaderResult<T>
	{
		public static ResultInstance<TypeReaderResult<T>, ITypeReaderResult> Failure { get; }
			= new(FromError());

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

		public static TypeReaderResult<T> FromError()
			=> new(ParseFailedResult<T>.Instance.Sync, default!);

		public static TypeReaderResult<T> FromError(IResult result)
			=> new(result, default!);

		public static TypeReaderResult<T> FromSuccess(T value)
			=> new(SuccessResult.Instance.Sync, value);
	}
}