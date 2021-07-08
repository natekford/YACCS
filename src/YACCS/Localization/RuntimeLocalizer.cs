using System.Collections.Generic;
using System.Globalization;

namespace YACCS.Localization
{
	public class RuntimeLocalizer : ILocalizer
	{
		public Localized<Dictionary<string, string>> Overrides { get; }
			= Localized.Create<Dictionary<string, string>>();

		public string? Get(string key, CultureInfo? culture = null)
			=> Overrides[culture].TryGetValue(key, out var value) ? value : null;
	}
}