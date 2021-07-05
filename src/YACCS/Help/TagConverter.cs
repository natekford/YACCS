using System;
using System.Globalization;

using YACCS.Localization;

namespace YACCS.Help
{
	public class TagConverter : ITagConverter
	{
		private readonly ILocalizer _Localizer;

		public string Separator { get; }
			= CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

		public TagConverter(ILocalizer localizer)
		{
			_Localizer = localizer;
		}

		public string Convert(TaggedString tagged)
		{
			var @string = tagged.String;
			if (!tagged.HasBeenLocalized)
			{
				@string = _Localizer.GetSafely(@string);
			}

			return tagged.Tag switch
			{
				Tag.String => @string,
				Tag.Header => @string + ": ",
				Tag.Key => @string + " = ",
				Tag.Value => @string,
				Tag.Space => " ",
				Tag.Newline => Environment.NewLine,
				Tag.ListSeparator => Separator,
				_ => @string,
			};
		}
	}
}