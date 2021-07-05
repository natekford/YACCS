using System.Globalization;
using System.Resources;

namespace YACCS.Localization
{
	public class ResourceManagerLocalizer : ILocalizer
	{
		private readonly ResourceManager _ResourceManager;

		public ResourceManagerLocalizer(ResourceManager resourceManager)
		{
			_ResourceManager = resourceManager;
		}

		public string? Get(string key, CultureInfo? culture = null)
			=> _ResourceManager.GetString(key, culture);
	}
}