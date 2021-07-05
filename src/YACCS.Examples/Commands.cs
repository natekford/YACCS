using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Interactivity.Input;
using YACCS.Results;

namespace YACCS.Examples
{
	public class Commands : ConsoleCommands<IContext>
	{
		[Command(nameof(ChangeUICulture))]
		public string ChangeUICulture(CultureInfo culture)
		{
			// This command effectively does nothing due to the way that
			// CultureInfo.CurrentX and async interact together
			CultureInfo.CurrentUICulture = culture;
			return $"Successfully changed the current UI culture to {culture}.";
		}

		[Command(nameof(Divide))]
		public int Divide(int numerator, [NotZero] int divisor)
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
		public void Time()
			=> Console.WriteLine($"The current time is: {DateTime.UtcNow}");

		[Command(nameof(Help))]
		public class Help : ConsoleCommands<IContext>
		{
			private static readonly string _Separator = new('-', 10);

			public ICommandService CommandService { get; set; } = null!;
			public IHelpFormatter HelpFormatter { get; set; } = null!;
			public IInput<IContext, string> Input { get; set; } = null!;

			[Command]
			public void HelpCommand()
			{
				var i = 0;
				foreach (var command in CommandService.Commands)
				{
					Console.WriteLine($"\t{++i}. {command.Names[0]}");
				}
			}

			[Command]
			public async Task<IResult> HelpCommand(IReadOnlyList<IImmutableCommand> commands)
			{
				var command = commands[0];
				if (commands.Count > 1)
				{
					Console.WriteLine("Enter the position of the command you want to see: ");
					var i = 0;
					foreach (var c in commands)
					{
						Console.WriteLine($"\t{++i}. {c.Names[0]}");
					}

					var options = new InputOptions<IContext, string, int>
					{
						Preconditions = new[]
						{
							new RangeParameterPrecondition(1, commands.Count)
						},
					};
					var result = await Input.GetAsync(Context, options).ConfigureAwait(false);
					if (!result.InnerResult.IsSuccess)
					{
						return result.InnerResult;
					}

					command = commands[result.Value - 1];
				}

				Console.WriteLine();
				Console.WriteLine(_Separator);
				var text = await HelpFormatter.FormatAsync(Context, command).ConfigureAwait(false);
				Console.WriteLine(text);
				Console.WriteLine(_Separator);
				return SuccessResult.Instance.Sync;
			}
		}
	}

	public abstract class ConsoleCommands<T> : CommandGroup<T> where T : IContext
	{
		public ConsoleHandler Console { get; set; } = null!;
	}
}