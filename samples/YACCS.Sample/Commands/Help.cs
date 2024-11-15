using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Help;
using YACCS.Interactivity;
using YACCS.Results;
using YACCS.Sample.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Sample.Commands;

[Command(nameof(Help))]
public class Help : ConsoleCommands
{
	private static readonly string _Separator = new('-', 10);

	[InjectService]
	public ICommandService CommandService { get; set; } = null!;
	[InjectService]
	public IHelpFormatter HelpFormatter { get; set; } = null!;

	[Disabled]
	public override string Abstract() => "21";

	[Command(nameof(Category))]
	public Task<IResult> Category(
		[CommandsCategoryTypeReader]
		[Remainder]
		IReadOnlyCollection<IImmutableCommand> commands)
		=> HelpCommand(commands);

	[Command]
	public Task<IResult> HelpCommand()
		=> HelpCommand(CommandService.Commands);

	[Command]
	public async Task<IResult> HelpCommand(
		[CommandsNameTypeReader]
		[Remainder]
		IReadOnlyCollection<IImmutableCommand> commands)
	{
		var executableCommands = new List<IImmutableCommand>(commands.Count);
		foreach (var c in commands.Where(x => !x.IsHidden && x.Source is null))
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
				Console.WriteLine($"\t{i + 1}. {executableCommands[i].Paths[0]}");
			}

			var result = await Input.GetAsync(Context, default(int), new()
			{
				Preconditions =
				[
					new RangeParameterPrecondition(1, executableCommands.Count)
				]
			}).ConfigureAwait(false);
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
		return CachedResults.Success;
	}
}