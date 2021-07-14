using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Examples.Commands
{
	public abstract class ConsoleCommands<T> : CommandGroup<T> where T : IContext
	{
		public ConsoleHandler Console { get; set; } = null!;

		[Command(nameof(Abstract), AllowInheritance = true)]
		public abstract string Abstract();
	}
}