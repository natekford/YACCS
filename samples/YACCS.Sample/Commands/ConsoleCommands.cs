using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Sample.Interactivity;

namespace YACCS.Sample.Commands;

public abstract class ConsoleCommands : CommandGroup<ConsoleContext>
{
	[InjectService]
	public ConsoleHandler Console { get; set; } = null!;
	[InjectService]
	public ConsoleInput Input { get; set; } = null!;
	[InjectService]
	public ConsolePaginator Paginator { get; set; } = null!;

	[Command(nameof(Abstract), AllowInheritance = true)]
	public abstract string Abstract();
}