using System.Threading.Tasks;

namespace YACCS.Results
{
	public class ExceptionAfterCommandResult : Result
	{
		public static IResult Instance { get; } = new ExceptionAfterCommandResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected ExceptionAfterCommandResult() : base(false, "An exception occurred after a command was executed.")
		{
		}
	}
}