namespace YACCS.Results
{
	public class TypeReaderResult : Result, ITypeReaderResult
	{
		public static TypeReaderResultInstance<TypeReaderResult> Failure { get; }
			= FromError().AsTypeReaderResultInstance();

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