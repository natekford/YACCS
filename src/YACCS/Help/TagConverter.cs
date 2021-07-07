using System;

using YACCS.Localization;

namespace YACCS.Help
{
	public class TagConverter : IFormatProvider, ICustomFormatter
	{
		protected ILocalizer? Localizer { get; }

		public TagConverter(ILocalizer? localizer)
		{
			Localizer = localizer;
		}

		public virtual string Format(
			string? format,
			object? arg,
			IFormatProvider formatProvider)
		{
			if (arg is null)
			{
				return "";
			}

			var tag = GetTags(format);
			var @string = arg.ToString();
			if (Localizer != null && (tag & Tag.IsLocalized) == 0)
			{
				@string = Localizer.GetSafely(@string);
			}
			return Convert(tag, @string);
		}

		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;

		protected static Tag GetTags(string? format)
		{
			var tag = Tag.Nothing;
			if (format is not null)
			{
				foreach (var c in format)
				{
					if (c == 'h')
					{
						tag |= Tag.Header;
					}
					else if (c == 'k')
					{
						tag |= Tag.Key;
					}
					else if (c == 'v')
					{
						tag |= Tag.Value;
					}
					else if (c == 'l')
					{
						tag |= Tag.IsLocalized;
					}
				}
			}
			return tag;
		}

		protected virtual string Convert(Tag tag, string @string)
		{
			if ((tag & Tag.Header) != 0)
			{
				@string = $"{@string}:";
			}
			if ((tag & Tag.Key) != 0)
			{
				@string = $"{@string} =";
			}
			return @string;
		}

		[Flags]
		protected enum Tag : ulong
		{
			Nothing = 0,
			Header = 1U << 1,
			Key = 1U << 2,
			Value = 1U << 3,
			IsLocalized = 1U << 4,
		}
	}
}