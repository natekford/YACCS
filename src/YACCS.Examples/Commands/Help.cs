﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Interactivity.Input;
using YACCS.Results;
using YACCS.Interactivity;

namespace YACCS.Examples
{

	[Command(nameof(Help))]
	public class Help : ConsoleCommands<IContext>
	{
		private static readonly string _Separator = new('-', 10);

		public ICommandService CommandService { get; set; } = null!;
		public IHelpFormatter HelpFormatter { get; set; } = null!;
		public IInput<IContext, string> Input { get; set; } = null!;

		public override string Abstract() => "21";

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

				var options = Input.CreateOptions().With(preconditions: new[]
				{
						new RangeParameterPrecondition(1, commands.Count)
					});
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
			return SuccessResult.Instance;
		}
	}
}