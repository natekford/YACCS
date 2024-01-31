using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Localization;
using YACCS.Preconditions;
using YACCS.Results;

namespace YACCS.Examples.Preconditions;

[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = true, Inherited = true)]
public class RequiresDay(DayOfWeek day)
	: Precondition<IContext>, IRuntimeFormattableAttribute
{
	public DayOfWeek Day { get; } = day;
	public virtual string FallbackErrorMessage { get; set; } = "Must be {0}.";

	public override ValueTask<IResult> CheckAsync(IImmutableCommand command, IContext context)
	{
		// Use the current time zone's day
		if (DateTime.Now.DayOfWeek != Day)
		{
			return new(new Failure(GetErrorMessage()));
		}
		return new(Success.Instance);
	}

	public ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(GetErrorMessage());

	private string GetErrorMessage()
		=> string.Format(Localize.This(nameof(RequiresDay), FallbackErrorMessage), Day);
}