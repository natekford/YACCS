using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace YACCS.Localization
{
	public class ResourceManagerLocalizer : ILocalizer
	{
		public Localized<Dictionary<string, string>> Overrides { get; } = Localized.Create<Dictionary<string, string>>();
		public List<ResourceManager> ResourceManagers { get; } = new();

		public string? Get(string key, CultureInfo? culture = null)
		{
			culture ??= CultureInfo.CurrentUICulture;

			var dict = Overrides.Get(culture);
			if (dict.TryGetValue(key, out var value))
			{
				return value;
			}

			foreach (var resourceManager in ResourceManagers)
			{
				if (resourceManager.GetString(key) is string s)
				{
					return s;
				}
			}

			return null;
		}
	}
}