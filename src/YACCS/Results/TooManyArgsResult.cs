using System.Threading.Tasks;

namespace YACCS.Results
{
	public class TooManyArgsResult : Result
	{
		public static IResult Instance { get; } = new TooManyArgsResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected TooManyArgsResult() : base(false, "Too many arguments provided.")
		{
		}
	}
}