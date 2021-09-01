using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Examples.Interactivity;

namespace YACCS.Examples.Commands
{
	[OnConsoleCommandsBuilding]
	public abstract class ConsoleCommands : CommandGroup<ConsoleContext>
	{
		public ConsoleHandler Console { get; set; } = null!;
		public ConsoleInput Input { get; set; } = null!;
		public ConsolePaginator Paginator { get; set; } = null!;

		[Command(nameof(Abstract), AllowInheritance = true)]
		public abstract string Abstract();

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		protected class OnConsoleCommandsBuildingAttribute : OnCommandBuildingAttribute
		{
			public override Task ModifyCommands(IServiceProvider services, List<ReflectionCommand> commands)
				=> throw new InvalidOperationException("Should not have been reached.");
		}
	}
}