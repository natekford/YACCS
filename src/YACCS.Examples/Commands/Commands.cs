using System;
using System.Collections.Generic;
using System.Globalization;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Examples.Preconditions;
using YACCS.Help.Attributes;
using YACCS.Results;

namespace YACCS.Examples.Commands
{
	public class Commands : ConsoleCommands
	{
		public override string Abstract() => "What's 9 + 10?";

		[Command(nameof(ChangeUICulture))]
		public string ChangeUICulture(CultureInfo culture)
		{
			// This command effectively does nothing due to the way that
			// CultureInfo.CurrentX and async interact together
			CultureInfo.CurrentUICulture = culture;
			return $"Successfully changed the current UI culture to {culture}.";
		}

		[Command(nameof(Divide), "D")]
		[Summary("Divides two numbers and returns the result (rounded down).")]
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

		[Command(nameof(Result))]
		public IResult Result()
			=> new SuccessResult("i give a success result for no reason");

		[Command(nameof(Throws))]
		public void Throws()
			=> throw new InvalidOperationException("i throw for no reason");

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