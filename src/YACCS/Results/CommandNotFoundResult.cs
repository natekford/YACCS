using System.Threading.Tasks;

namespace YACCS.Results
{
	public class CommandNotFoundResult : Result
	{
		public static IResult Instance { get; } = new CommandNotFoundResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected CommandNotFoundResult() : base(false, "Unable to find a matching command.")
		{
		}
	}
}