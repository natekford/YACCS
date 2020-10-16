using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Results;

namespace YACCS.Examples
{
	public class Commands : CommandGroup<ConsoleContext>
	{
		[Command(nameof(Echo))]
		public void Echo([Remainder] string input)
			=> Console.WriteLine(input);

		[Command(nameof(Exit))]
		public void Exit()
			=> Environment.Exit(0);

		[Command(nameof(Result))]
		public IResult Result()
			=> new Result(true, "i give a success result for no reason");

		[Command(nameof(Throws))]
		public void Throws()
			=> throw new InvalidOperationException("i throw for no reason");

		[Command(nameof(Time))]
		public void Time()
			=> Console.WriteLine($"The current time is: {DateTime.UtcNow}");

		[Command(nameof(Help))]
		public class Help : CommandGroup<ConsoleContext>
		{
			public ICommandService CommandService { get; set; }
			public IHelpFormatter HelpService { get; set; }

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
			public async Task HelpCommand(IReadOnlyList<IImmutableCommand> commands)
			{
				foreach (var command in commands)
				{
					var text = await HelpService.FormatAsync(Context, command).ConfigureAwait(false);
					Console.WriteLine(text);
				}
			}
		}
	}
}