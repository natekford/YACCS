using System;
using System.Collections.Generic;
using System.Globalization;

namespace YACCS.Help
{
	public class TagConverter : IFormatProvider, ICustomFormatter
	{
		protected virtual Dictionary<string, Func<string, string>> Formatters { get; } = new(StringComparer.OrdinalIgnoreCase)
		{
			[Tag.Header] = x => $"{x}:",
			[Tag.Key] = x => $"{x} =",
			[Tag.Value] = x => x,
		};

		public virtual string Format(
			string? format,
			object? arg,
			IFormatProvider formatProvider)
		{
			if (arg is null)
			{
				return "";
			}
			if (format is not null)
			{
				if (Formatters.TryGetValue(format, out var formatter))
				{
					return formatter(arg.ToString());
				}
				else if (arg is IFormattable formattable)
				{
					return formattable.ToString(format, CultureInfo.CurrentCulture);
				}
			}
			return arg.ToString();
		}

		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;
	}
}