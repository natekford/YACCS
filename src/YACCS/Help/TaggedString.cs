using System;
using System.Globalization;

namespace YACCS.Help
{
	public readonly struct TaggedString
	{
		public static TaggedString ListSeparator
		{
			get
			{
				var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
				return new(Tag.ListSeparator, separator, hasBeenLocalized: true);
			}
		}
		public static TaggedString Newline { get; }
			= new(Tag.Newline, Environment.NewLine, hasBeenLocalized: true);
		public static TaggedString Space { get; }
			= new(Tag.Space, " ");

		public bool HasBeenLocalized { get; }
		public string String { get; }
		public Tag Tag { get; }

		public TaggedString(Tag tag, string @string, bool hasBeenLocalized = false)
		{
			Tag = tag;
			String = @string;
			HasBeenLocalized = hasBeenLocalized;
		}
	}
}