using System.Threading.Tasks;

namespace YACCS.Results
{
	public class NullParameterResult : Result
	{
		public static IResult Instance { get; } = new NullParameterResult();
		public static Task<IResult> InstanceTask { get; } = Instance.AsTask();

		protected NullParameterResult() : base(false, "Parameter is null.")
		{
		}
	}
}