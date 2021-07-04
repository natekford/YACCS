using System.Globalization;

namespace YACCS.Localization
{
	public interface ILocalizer
	{
		string? Get(string key, CultureInfo? culture = null);
	}
}