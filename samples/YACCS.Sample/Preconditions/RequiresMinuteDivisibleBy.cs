using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class RequiresMinuteDivisibleBy(int divisor)
	: SummarizablePrecondition<IContext>, ISummarizableAttribute
{
	public int Divisor { get; } = divisor;

	public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
	{
		if (DateTime.Now.Minute % Divisor != 0)
		{
			return new(Result.Failure(GetErrorMessage()));
		}
		return new(Result.EmptySuccess);
	}

	public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	private string GetErrorMessage()
	{
		return string.Format(Localize.This(
			key: nameof(RequiresMinuteDivisibleBy),
			fallback: "Current minute must be divisible by {0}."
		), Divisor);
	}
}