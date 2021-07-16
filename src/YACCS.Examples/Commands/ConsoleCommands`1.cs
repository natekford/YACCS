using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Examples.Commands
{
	public abstract class ConsoleCommands : CommandGroup<ConsoleContext>
	{
		public ConsoleHandler Console { get; set; } = null!;
		public ConsoleInput Input { get; set; } = null!;

		[Command(nameof(Abstract), AllowInheritance = true)]
		public abstract string Abstract();
	}
}