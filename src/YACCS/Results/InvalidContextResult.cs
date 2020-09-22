using System.Threading.Tasks;

namespace YACCS.Results
{
	public class InvalidContextResult : Result
	{
		public static IResult Instance { get; } = new InvalidContextResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected InvalidContextResult() : base(false, "Invalid context type.")
		{
		}
	}
}