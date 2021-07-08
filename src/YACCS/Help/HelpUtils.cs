using System;
using System.Text;

namespace YACCS.Help
{
	internal static class HelpUtils
	{
		internal static StringBuilder AppendDepth(this StringBuilder sb, int depth)
		{
			for (var i = 0; i < depth; ++i)
			{
				sb.Append('\t');
			}
			return sb;
		}

		internal static string Format(this IFormatProvider? formatProvider, FormattableString @string)
			=> formatProvider is null ? @string.ToString() : @string.ToString(formatProvider);
	}
}