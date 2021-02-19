using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class NotZero : ParameterPreconditionAttribute
	{
		private static readonly Task<IResult> _IsZero = Result.FromError("Cannot be zero.").AsTask();

		public static Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			int value)
		{
			if (value == 0)
			{
				return _IsZero;
			}
			return SuccessResult.Instance.Task;
		}

		protected override Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			object? value)
			=> this.CheckAsync<IContext, int>(parameter, context, value, CheckAsync);
	}
}