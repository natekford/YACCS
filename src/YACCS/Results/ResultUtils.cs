using System.Diagnostics.CodeAnalysis;

namespace YACCS.Results
{
	public static class ResultUtils
	{
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
			this IResult result,
			[NotNullWhen(true)] out T value)
		{
			if (result is ValueResult vResult && vResult.Value is T t)
			{
				value = t;
				return true;
			}
			if (result is INestedResult nResult)
			{
				return nResult.InnerResult.TryGetValue(out value);
			}
			value = default!;
			return false;
		}
	}
}