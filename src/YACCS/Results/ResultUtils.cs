using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace YACCS.Results
{
	public static class ResultUtils
	{
		public static Task<IResult> AsTask(this IResult result)
			=> Task.FromResult(result);

		public static Task<ITypeReaderResult<T>> AsTask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result);

		public static Task<ITypeReaderResult> AsTask(this ITypeReaderResult result)
			=> Task.FromResult(result);

		public static IResult GetMostNestedResult(this INestedResult result)
		{
			var actual = result.Result;
			if (actual is INestedResult nested)
			{
				return nested.GetMostNestedResult();
			}
			return actual;
		}

		public static bool TryGetValue<T>(
			this ExecutionResult result,
			[NotNullWhen(true)] out T value)
		{
			if (result.Result is ValueResult vResult && vResult.Value is T t)
			{
				value = t;
				return true;
			}
			value = default!;
			return false;
		}
	}
}