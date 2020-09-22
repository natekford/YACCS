using System.Threading.Tasks;

namespace YACCS.Results
{
	public class TypeReaderResult : Result, ITypeReaderResult
	{
		public static ITypeReaderResult Failure { get; } = FromError();
		public static Task<ITypeReaderResult> FailureTask { get; } = Failure.AsTask();

		public object? Arg { get; }

		public TypeReaderResult(bool isSuccess, string response, object? arg)
			: base(isSuccess, response)
		{
			Arg = arg;
		}

		public static TypeReaderResult FromError()
			=> new TypeReaderResult(false, "", null);

		public static TypeReaderResult FromSuccess(object arg)
			=> new TypeReaderResult(true, "", arg);
	}
}