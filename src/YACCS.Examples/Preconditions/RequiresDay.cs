using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions;

[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class RequiresDay : Precondition<IContext>, IRuntimeFormattableAttribute
{
	public DayOfWeek Day { get; }
	public virtual string FallbackErrorMessage { get; set; } = "Must be {0}.";

	public RequiresDay(DayOfWeek day)
	{
		Day = day;
	}

	public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
	{
		// Use the current time zone's day
		if (DateTime.Now.DayOfWeek != Day)
		{
			return new(new FailureResult(GetErrorMessage()));
		}
		return new(SuccessResult.Instance);
	}

	public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	private string GetErrorMessage()
		=> string.Format(Localize.This(nameof(RequiresDay), FallbackErrorMessage), Day);
}
