using System;
using System.Globalization;

namespace YACCS.Help
{
	public class TagConverter : ITagConverter
	{
		public string Separator { get; }
			= CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

		public string Convert(TaggedString tagged)
		{
			return tagged.Tag switch
			{
				Tag.String => tagged.String,
				Tag.Header => tagged.String + ": ",
				Tag.Key => tagged.String + " = ",
				Tag.Value => tagged.String,
				Tag.Space => " ",
				Tag.Newline => Environment.NewLine,
				Tag.ListSeparator => Separator,
				_ => tagged.String,
			};
		}
	}
}