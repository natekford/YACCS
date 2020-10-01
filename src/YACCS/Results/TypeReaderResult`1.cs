using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	public class TypeReaderResult<T> : TypeReaderResult, ITypeReaderResult<T>
	{
		public new static TypeReaderResultInstance<TypeReaderResult<T>> Failure { get; }
			= FromError().AsTypeReaderResultInstance();

		[MaybeNull]
		public new T Value { get; }

		public TypeReaderResult(bool isSuccess, string response, [MaybeNull] T value)
			: base(isSuccess, response, value)
		{
			Value = value;
		}

		public new static TypeReaderResult<T> FromError()
			=> new TypeReaderResult<T>(false, "", default!);

		public static TypeReaderResult<T> FromSuccess(T value)
			=> new TypeReaderResult<T>(true, "", value);
	}
}