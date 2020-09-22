using System.Threading.Tasks;

namespace YACCS.Results
{
	public class QuoteMismatchResult : Result
	{
		public static IResult Instance { get; } = new QuoteMismatchResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected QuoteMismatchResult() : base(false, "Unable to parse arguments: quote mismatch.")
		{
		}
	}
}