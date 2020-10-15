using System;
using System.Globalization;

namespace YACCS.Help
{
	public readonly struct TaggedString
	{
		public static TaggedString ListSeparator { get; }
			= new TaggedString(Tag.ListSeparator, CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ");
		public static TaggedString Newline { get; }
			= new TaggedString(Tag.Newline, Environment.NewLine);
		public static TaggedString Space { get; }
			= new TaggedString(Tag.Space, " ");

		public string String { get; }
		public Tag Tag { get; }

		public TaggedString(Tag tag, string @string)
		{
			Tag = tag;
			String = @string;
		}
	}
}