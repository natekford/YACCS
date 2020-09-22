using System.Threading.Tasks;

namespace YACCS.Results
{
	public class SuccessResult : Result
	{
		public static IResult Instance { get; } = new SuccessResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected SuccessResult() : base(true, "")
		{
		}
	}
}