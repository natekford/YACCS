using System.Reflection;

using YACCS.Help.Attributes;

namespace YACCS.Help.Models
{
	public static class HelpItemUtils
	{
		public static HelpItem<T> Create<T>(T item) where T : ICustomAttributeProvider
			=> new(item, item.GetCustomAttributes(true));

		public static bool IsAsyncFormattable(this IHelpCommand item)
			=> item.IsAsync() || item.HasAsyncFormattableParameters || item.HasAsyncFormattablePreconditions;

		public static bool IsAsyncFormattable(this IHelpParameter item)
			=> item.IsAsync() || item.HasAsyncFormattablePreconditions;

		public static bool IsAsyncFormattable(this IHelpItem<object> item)
			=> item.IsAsync();

		private static bool IsAsync(this IHelpItem<object> item)
			=> item.HasAsyncFormattableAttributes || item.Item is IAsyncRuntimeFormattableAttribute;
	}
}