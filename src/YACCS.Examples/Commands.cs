using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
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
			public void HelpCommand(IReadOnlyList<IImmutableCommand> commands)
			{
				foreach (var command in commands)
				{
					var parameters = command.Parameters
						.Select(x => $"{x.ParameterType} {x.ParameterName}");
					Console.WriteLine($"\t{command.Names[0]}({string.Join(", ", parameters)})");
				}
			}
		}
	}
}