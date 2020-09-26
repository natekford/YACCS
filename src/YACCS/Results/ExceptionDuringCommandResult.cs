using System.Threading.Tasks;

namespace YACCS.Results
{
	public class ExceptionDuringCommandResult : Result
	{
		public static IResult Instance { get; } = new ExceptionDuringCommandResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected ExceptionDuringCommandResult() : base(false, "An exception occurred while a command was executing.")
		{
		}
	}
}