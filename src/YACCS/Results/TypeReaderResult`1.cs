using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace YACCS.Results
{
	public class TypeReaderResult<T> : TypeReaderResult, ITypeReaderResult<T>
	{
		public new static ITypeReaderResult<T> Failure { get; } = TypeReaderResult<T>.FromError();
		public new static Task<ITypeReaderResult<T>> FailureTask { get; } = Failure.AsTask();

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