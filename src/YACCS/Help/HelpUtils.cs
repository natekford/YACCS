using System;

namespace YACCS.Help
{
	public static class HelpUtils
	{
		internal static string Format(this IFormatProvider? formatProvider, FormattableString @string)
			=> formatProvider is null ? @string.ToString() : @string.ToString(formatProvider);
	}
}