using YACCS.Commands;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

public class RangeParameterPrecondition : ParameterPrecondition<IContext, int>
{
	private readonly int _Max;
	private readonly int _Min;
	private readonly IResult _TooHigh;
	private readonly IResult _TooLow;

	public RangeParameterPrecondition(int min, int max)
	{
		_Max = max;
		_TooHigh = Result.MustBeLessThan(_Max);
		_Min = min;
		_TooLow = Result.MustBeGreaterThan(_Min);
	}

	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IContext context,
		int value)
	{
		if (value < _Min)
		{
			return new(_TooLow);
		}
		if (value > _Max)
		{
			return new(_TooHigh);
		}
		return new(Result.EmptySuccess);
	}
}