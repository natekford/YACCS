using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class TypeReaderResult<T> : Result, ITypeReaderResult<T>
	{
		public static ResultInstance<TypeReaderResult<T>, ITypeReaderResult> Failure { get; }
			= FromError().AsTypeReaderResultInstance();

		[MaybeNull]
		public T Value { get; }
		object? ITypeReaderResult.Value => Value;
		private string DebuggerDisplay => $"IsSuccess = {IsSuccess}, Response = {Response}, Value = {Value}";

		public TypeReaderResult(bool isSuccess, string response, [MaybeNull] T value)
			: base(isSuccess, response)
		{
			Value = value;
		}

		public static TypeReaderResult<T> FromError()
			=> new TypeReaderResult<T>(false, $"Failed to parse {typeof(T).Name}.", default!);

		public static TypeReaderResult<T> FromSuccess(T value)
			=> new TypeReaderResult<T>(true, "", value);
	}
}