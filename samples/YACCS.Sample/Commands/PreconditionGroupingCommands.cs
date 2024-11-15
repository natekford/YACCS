using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Sample.Preconditions;

namespace YACCS.Sample.Commands;

public class PreconditionGroupingCommands : ConsoleCommands
{
	[Disabled]
	public override string Abstract()
		=> "Words.";

	[Command(nameof(AWeekendDay))]
	[RequiresDay(DayOfWeek.Saturday, Op = Op.Or)]
	[RequiresDay(DayOfWeek.Sunday, Op = Op.Or)]
	public string AWeekendDay()
		=> DateTime.Now.DayOfWeek.ToString();

	[Command(nameof(AWeekendDayAndAnEvenMinute))]
	[RequiresDay(DayOfWeek.Saturday, Groups = ["Day"], Op = Op.Or)]
	[RequiresDay(DayOfWeek.Sunday, Groups = ["Day"], Op = Op.Or)]
	[RequiresMinuteDivisibleBy(2)]
	public string AWeekendDayAndAnEvenMinute()
		=> DateTime.Now.ToString("G");

	[Command(nameof(BothWeekendDays))]
	[RequiresDay(DayOfWeek.Saturday)]
	[RequiresDay(DayOfWeek.Sunday)]
	public string BothWeekendDays()
		=> throw new InvalidOperationException("This should not be possible to execute.");
}