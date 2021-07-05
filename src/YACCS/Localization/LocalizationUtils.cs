using System.Collections.Generic;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.Localization
{
	public static class LocalizationUtils
	{
		public static string GetLocalizedString(
			this IContext context,
			string key,
			string? @default = null)
		{
			var localizer = context.Services.GetService<ILocalizer>();
			return localizer?.Get(key) ?? @default ?? key;
		}

		public static void InjectLocalizer(
			this IImmutableCommand command,
			ILocalizer localizer)
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

		public static T InjectLocalizer<T>(this T commands, ILocalizer localizer)
			where T : IEnumerable<IImmutableCommand>
		{
			foreach (var command in commands)
			{
				command.InjectLocalizer(localizer);
			}
			return commands;
		}

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