using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using MorseCode.ITask;

using static YACCS.Results.Result;

namespace YACCS.Results
{
	public static class ResultUtils
	{
		public static ITask<IResult> AsITask(this IResult result)
			=> Task.FromResult(result).AsITask();

		public static ResultInstance<T, IResult> AsResultInstance<T>(this T instance) where T : IResult
			=> new ResultInstance<T, IResult>(instance);

		public static Task<IResult> AsTask(this IResult result)
			=> Task.FromResult(result);

		public static IResult GetMostNestedResult(this INestedResult result)
		{
			var actual = result.InnerResult;
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
			if (result.InnerResult is ValueResult vResult && vResult.Value is T t)
			{
				value = t;
				return true;
			}
			value = default!;
			return false;
		}
	}
}