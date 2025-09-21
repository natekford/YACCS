using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Sample.Preconditions;

[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class RequiresDay(DayOfWeek day)
	: Precondition<IContext>, ISummarizableAttribute
{
	public DayOfWeek Day { get; } = day;

	public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
	{
		// Use the current time zone's day
		if (DateTime.Now.DayOfWeek != Day)
		{
			return new(Result.Failure(GetErrorMessage()));
		}
		return new(Result.EmptySuccess);
	}

	public override ValueTask<string> GetSummaryAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	private string GetErrorMessage()
		=> string.Format(Localize.This(nameof(RequiresDay), "Must be {0}."), Day);
}