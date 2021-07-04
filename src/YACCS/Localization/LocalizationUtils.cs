using System.Collections.Generic;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Localization
{
	public static class LocalizationUtils
	{
		public static void InjectInto(
			this ILocalizer localizer,
			IEnumerable<IImmutableCommand> commands)
		{
			foreach (var command in commands)
			{
				localizer.InjectInto(command);
			}
		}

		public static void InjectInto(
			this ILocalizer localizer,
			IImmutableCommand command)
		{
			localizer.InjectInto(command.Attributes);
			foreach (var parameter in command.Parameters)
			{
				localizer.InjectInto(parameter.Attributes);
			}
		}

		public static void InjectResourceManager(
			this CommandService commands,
			ILocalizer localizer)
			=> localizer.InjectInto(commands.Commands.Items);

		private static void InjectInto(
			this ILocalizer localizer,
			IReadOnlyList<object> attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute is IUsesLocalizer iurm)
				{
					iurm.Localizer = localizer;
				}
			}
		}
	}
}