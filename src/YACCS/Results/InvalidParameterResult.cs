using System.Threading.Tasks;

namespace YACCS.Results
{
	public class InvalidParameterResult : Result
	{
		public static IResult Instance { get; } = new InvalidParameterResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected InvalidParameterResult() : base(false, "Invalid parameter type.")
		{
		}
	}
}