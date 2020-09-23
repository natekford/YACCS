using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Commands.Linq
{
	public static class Commands
	{
		public static ICommand AddName(this ICommand command, IName name)
		{
			command.Names.Add(name);
			return command;
		}

		public static ICommand AddPrecondition<TPrecon>(
			this ICommand command,
			TPrecon precondition)
			where TPrecon : IPrecondition
		{
			command.Attributes.Add(precondition);
			return command;
		}
	}
}