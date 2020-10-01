using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	public class TypeReaderResult<T> : TypeReaderResult, ITypeReaderResult<T>
	{
		public new static TypeReaderResultInstance<TypeReaderResult<T>> Failure { get; }
			= FromError().AsTypeReaderResultInstance();

		[MaybeNull]
		public new T Arg { get; }

		public TypeReaderResult(bool isSuccess, string response, [MaybeNull] T arg)
			: base(isSuccess, response, arg)
		{
			Arg = arg;
		}

		public new static TypeReaderResult<T> FromError()
			=> new TypeReaderResult<T>(false, "", default!);

		public static TypeReaderResult<T> FromSuccess(T arg)
			=> new TypeReaderResult<T>(true, "", arg);
	}
}