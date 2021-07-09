using System;
using System.Text;

namespace YACCS.Help
{
	internal static class HelpUtils
	{
		internal static string Format(this IFormatProvider? formatProvider, FormattableString @string)
			=> formatProvider is null ? @string.ToString() : @string.ToString(formatProvider);
	}
}