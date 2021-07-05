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
			localizer.InjectInto(command);
			localizer.InjectInto(command.Attributes);
			localizer.InjectInto(command.Names);
			foreach (var (_, preconditions) in command.Preconditions)
			{
				localizer.InjectInto(preconditions);
			}
			foreach (var parameter in command.Parameters)
			{
				localizer.InjectInto(parameter);
				localizer.InjectInto(parameter.Attributes);
				localizer.InjectInto(parameter.DefaultValue);
				localizer.InjectInto(parameter.Preconditions);
				localizer.InjectInto(parameter.TypeReader);
			}
		}

		public static void InjectLocalizer(
			this CommandService commands,
			ILocalizer localizer)
			=> localizer.InjectInto(commands.Commands.Items);

		private static void InjectInto(
			this ILocalizer localizer,
			IEnumerable<object> objects)
		{
			foreach (var attribute in objects)
			{
				localizer.InjectInto(attribute);
			}
		}

		private static void InjectInto(
			this ILocalizer localizer,
			object? @object)
		{
			if (@object is IUsesLocalizer canLocalize)
			{
				canLocalize.Localizer = localizer;
			}
		}
	}
}