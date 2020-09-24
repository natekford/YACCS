using System.Threading.Tasks;

namespace YACCS.Results
{
	public class MultiMatchHandlingErrorResult : Result
	{
		public static IResult Instance { get; } = new MultiMatchHandlingErrorResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected MultiMatchHandlingErrorResult() : base(false, "Multiple commands match.")
		{
		}
	}
}