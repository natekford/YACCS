using System.Threading.Tasks;

namespace YACCS.Results
{
	public class NotEnoughArgsResult : Result
	{
		public static IResult Instance { get; } = new NotEnoughArgsResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected NotEnoughArgsResult() : base(false, "Not enough arguments provided.")
		{
		}
	}
}