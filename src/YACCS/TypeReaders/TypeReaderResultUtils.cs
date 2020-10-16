using System.Threading.Tasks;

using MorseCode.ITask;

using static YACCS.Results.Result;

namespace YACCS.TypeReaders
{
	public static class TypeReaderResultUtils
	{
		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static ITask<ITypeReaderResult> AsITask(this ITypeReaderResult result)
			=> Task.FromResult(result).AsITask();

		public static Task<ITypeReaderResult<T>> AsTask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result);

		public static Task<ITypeReaderResult> AsTask(this ITypeReaderResult result)
			=> Task.FromResult(result);

		public static ResultInstance<T, ITypeReaderResult> AsTypeReaderResultInstance<T>(this T instance) where T : ITypeReaderResult
			=> new ResultInstance<T, ITypeReaderResult>(instance);
	}
}