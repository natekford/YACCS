using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class RangeParameterPrecondition : ParameterPrecondition<IContext, int>
	{
		private readonly int _Max;
		private readonly Task<IResult> _MaxResult;
		private readonly int _Min;
		private readonly Task<IResult> _MinResult;

		public RangeParameterPrecondition(int min, int max)
		{
			_Max = max;
			_MaxResult = Result.FromError($"Must be less than or equal to {_Max}.").AsTask();
			_Min = min;
			_MinResult = Result.FromError($"Must be greater or equal to {_Min}.").AsTask();
		}

		public override Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] int value)
		{
			if (value < _Min)
			{
				return _MinResult;
			}
			if (value > _Max)
			{
				return _MaxResult;
			}
			return SuccessResult.Instance.Task;
		}
	}
}