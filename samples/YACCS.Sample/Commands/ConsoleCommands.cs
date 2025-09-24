using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Commands.Models;
using YACCS.Sample.Interactivity;

namespace YACCS.Sample.Commands;

public abstract class ConsoleCommands : CommandGroup<ConsoleContext>
{
	[InjectService]
	public required ConsoleHandler Console { get; set; }
	[InjectService]
	public required ConsoleInput Input { get; set; }
	[InjectService]
	public required ConsolePaginator Paginator { get; set; }

	[Command(nameof(Abstract), AllowInheritance = true)]
	public abstract string Abstract();
}