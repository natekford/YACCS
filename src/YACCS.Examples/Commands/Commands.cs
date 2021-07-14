using System;
using System.Collections.Generic;
using System.Globalization;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Results;

namespace YACCS.Examples
{
	public class Commands : ConsoleCommands<IContext>
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

		[Command(nameof(Divide))]
		[Summary("Divides two numbers and returns the result (rounded down).")]
		public int Divide(
			[Summary("The number being divided.")]
			int numerator,
			[Summary("The number to divide by.")]
			[NotZero]
			int divisor)
			=> numerator / divisor;

		[Command(nameof(Echo))]
		public void Echo([Remainder] string input)
			=> Console.WriteLine(input);

		[Command(nameof(EchoMultipleLines))]
		public void EchoMultipleLines([Remainder] IEnumerable<string> input)
		{
			foreach (var item in input)
			{
				Console.WriteLine(item);
			}
		}

		[Command(nameof(Exit))]
		public void Exit()
			=> Environment.Exit(0);

		[Command(nameof(Result))]
		public IResult Result()
			=> new SuccessResult("i give a success result for no reason");

		[Command(nameof(Throws))]
		public void Throws()
			=> throw new InvalidOperationException("i throw for no reason");

		[Command(nameof(Time))]
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