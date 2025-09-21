using YACCS.Commands;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

public class NotZero : ParameterPrecondition<IContext, int>, ISummarizableAttribute
{
	public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IContext context,
		int value)
	{
		if (value == 0)
		{
			return new(Result.Failure(GetErrorMessage()));
		}
		return new(Result.EmptySuccess);
	}

	private string GetErrorMessage()
		=> Localize.This(nameof(NotZero), "Cannot be zero.");
}