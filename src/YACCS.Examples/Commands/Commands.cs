using System.Globalization;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Examples.Preconditions;
using YACCS.Help.Attributes;
using YACCS.Interactivity;
using YACCS.NamedArguments;
using YACCS.Results;

namespace YACCS.Examples.Commands
{
	public class Commands : ConsoleCommands
	{
		public override string Abstract() => "What's 9 + 10?";

		[Command(nameof(Delay))]
		public async Task<string> Delay()
		{
			await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
			return "I delayed for 10 seconds.";
		}

		[Command(nameof(Divide), "D")]
		[Summary("Divides two numbers and returns the result (rounded down).")]
		[GenerateNamedArguments]
		public int Divide(
			[Summary("The number being divided.")]
			int numerator,
			[Summary("The number to divide by.")]
			[NotZero]
			int divisor)
			=> numerator / divisor;

		[Command(nameof(Echo), "E")]
		public string Echo([Remainder] string input)
			=> input;

		[Command(nameof(EchoDistinctLines), "EDL")]
		public string EchoDistinctLines([Remainder] HashSet<string> input)
			=> string.Join('\n', input);

		[Command(nameof(EchoMultipleLines), "EML")]
		public string EchoMultipleLines([Remainder] List<string> input)
			=> string.Join('\n', input);

		[Command(nameof(Exit))]
		public void Exit()
			=> Environment.Exit(0);

		[Command(nameof(Pagination))]
		public async Task Pagination()
		{
			var options = Paginator.CreateOptions().With(
				maxPage: 100,
				displayCallback: page =>
				{
					Console.WriteLine($"Page #{page}");
					return Task.CompletedTask;
				},
				startingPage: 0,
				timeout: TimeSpan.FromSeconds(30)
			);
			await Paginator.PaginateAsync(Context, options).ConfigureAwait(false);
		}

		[Command(nameof(Result))]
		public IResult Result()
			=> new SuccessResult("This command gives a success result for no reason.");

		[Command(nameof(Sleep))]
		public string Sleep()
		{
			Thread.Sleep(TimeSpan.FromSeconds(10));
			return "I slept for 10 seconds.";
		}

		[Command(nameof(Throws))]
		public void Throws()
			=> throw new InvalidOperationException("This command throws for no reason.");

		[Command(nameof(Time), "T")]
		[Summary("Prints out the current time in UTC, optionally converted to a specified timezone.")]
		public string Time(
			[Summary("The timezone to convert to, no input means UTC.")]
			TimeZoneInfo? timeZone = null)
		{
			var time = DateTime.UtcNow;
			if (timeZone is not null)
			{
				time = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);
			}
			return $"The current time is: {time}";
		}
	}
}