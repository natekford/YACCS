using YACCS.Commands.Attributes;
using YACCS.Examples.Preconditions;
using YACCS.Preconditions;

namespace YACCS.Examples.Commands
{
	public class PreconditionGroupingCommands : ConsoleCommands
	{
		[Disabled]
		public override string Abstract()
			=> "Words.";

		[Command(nameof(AWeekendDay))]
		[RequiresDay(DayOfWeek.Saturday, Op = BoolOp.Or)]
		[RequiresDay(DayOfWeek.Sunday, Op = BoolOp.Or)]
		public string AWeekendDay()
			=> DateTime.Now.DayOfWeek.ToString();

		[Command(nameof(AWeekendDayAndAnEvenMinute))]
		[RequiresDay(DayOfWeek.Saturday, Groups = new[] { "Day" }, Op = BoolOp.Or)]
		[RequiresDay(DayOfWeek.Sunday, Groups = new[] { "Day" }, Op = BoolOp.Or)]
		[RequiresMinuteDivisibleBy(2)]
		public string AWeekendDayAndAnEvenMinute()
			=> DateTime.Now.ToString("G");

		[Command(nameof(BothWeekendDays))]
		[RequiresDay(DayOfWeek.Saturday)]
		[RequiresDay(DayOfWeek.Sunday)]
		public string BothWeekendDays()
			=> throw new InvalidOperationException("This should not be possible to execute.");
	}
}