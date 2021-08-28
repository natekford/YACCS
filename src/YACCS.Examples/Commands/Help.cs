﻿using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Examples.Preconditions;
using YACCS.Help;
using YACCS.Interactivity;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Examples.Commands
{
	[Command(nameof(Help))]
	[OnHelpBuilding]
	public class Help : ConsoleCommands
	{
		private static readonly string _Separator = new('-', 10);

		public ICommandService CommandService { get; set; } = null!;
		public IHelpFormatter HelpFormatter { get; set; } = null!;

		[Disabled]
		public override string Abstract() => "21";

		[Command(nameof(Category))]
		public Task<IResult> Category(
			[OverrideTypeReader(typeof(CommandsCategoryTypeReader))]
			[Remainder]
			IReadOnlyCollection<IImmutableCommand> commands)
			=> HelpCommand(commands);

		[Command]
		public Task HelpCommand()
			=> HelpCommand(CommandService.Commands);

		[Command]
		public async Task<IResult> HelpCommand(
			[OverrideTypeReader(typeof(CommandsNameTypeReader))]
			[Remainder]
			IReadOnlyCollection<IImmutableCommand> commands)
		{
			var executableCommands = new List<IImmutableCommand>(commands.Count);
			foreach (var c in commands.Where(x => !x.IsHidden))
			{
				var canExecute = await c.CanExecuteAsync(Context).ConfigureAwait(false);
				if (canExecute.IsSuccess)
				{
					executableCommands.Add(c);
				}
			}

			IImmutableCommand command;
			if (executableCommands.Count == 0)
			{
				return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailed.Result.InnerResult;
			}
			else if (executableCommands.Count == 1)
			{
				command = executableCommands[0];
			}
			else
			{
				Console.WriteLine("Enter the position of the command you want to see: ");
				for (var i = 0; i < executableCommands.Count; ++i)
				{
					Console.WriteLine($"\t{i + 1}. {executableCommands[i].Names[0]}");
				}

				var options = Input.CreateOptions().With(preconditions: new[]
				{
						new RangeParameterPrecondition(1, executableCommands.Count)
				});
				var result = await Input.GetAsync(Context, options).ConfigureAwait(false);
				if (!result.InnerResult.IsSuccess)
				{
					return result.InnerResult;
				}

				command = executableCommands[result.Value - 1];
			}

			Console.WriteLine();
			Console.WriteLine(_Separator);
			var text = await HelpFormatter.FormatAsync(Context, command).ConfigureAwait(false);
			Console.WriteLine(text);
			Console.WriteLine(_Separator);
			return SuccessResult.Instance;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class OnHelpBuildingAttribute : OnCommandBuildingAttribute
	{
		public override Task ModifyCommands(IServiceProvider services, List<ReflectionCommand> commands)
		{
			Debug.WriteLine($"{nameof(OnHelpBuildingAttribute)}: {commands.Count} commands created.");
			return Task.CompletedTask;
		}
	}
}