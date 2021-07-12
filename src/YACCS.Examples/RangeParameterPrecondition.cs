using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples
{
	public class RangeParameterPrecondition : ParameterPrecondition<IContext, int>
	{
		private readonly int _Max;
		private readonly IResult _MaxResult;
		private readonly int _Min;
		private readonly IResult _MinResult;

		public RangeParameterPrecondition(int min, int max)
		{
			_Max = max;
			_MaxResult = new FailureResult($"Must be less than or equal to {_Max}.");
			_Min = min;
			_MinResult = new FailureResult($"Must be greater or equal to {_Min}.");
		}

		public override ValueTask<IResult> CheckAsync(
			IImmutableCommand command,
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] int value)
		{
			if (value < _Min)
			{
				return new(_MinResult);
			}
			if (value > _Max)
			{
				return new(_MaxResult);
			}
			return new(SuccessResult.Instance.Sync);
		}
	}
}