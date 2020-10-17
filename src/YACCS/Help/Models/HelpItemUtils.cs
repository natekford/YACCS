using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models
{
	public static class HelpItemUtils
	{
		public static HelpItem<T> Create<T>(T item) where T : ICustomAttributeProvider
			=> new HelpItem<T>(item, item.GetCustomAttributes(true));

		public static bool IsAsyncFormattable(this IHelpCommand command)
			=> command.HasAsyncFormattableAttributes || command.HasAsyncFormattableParameters || command.HasAsyncFormattablePreconditions;

		public static bool IsAsyncFormattable(this IHelpParameter parameter)
			=> parameter.HasAsyncFormattableAttributes || parameter.HasAsyncFormattablePreconditions;
	}
}