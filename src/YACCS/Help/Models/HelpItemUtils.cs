using System.Reflection;

namespace YACCS.Help.Models
{
	public static class HelpItemUtils
	{
		public static HelpItem<T> Create<T>(T item) where T : ICustomAttributeProvider
			=> new(item, item.GetCustomAttributes(true));
	}
}