using YACCS.Localization;

namespace YACCS.Help
{
	public class MarkdownTagConverter : TagConverter
	{
		public MarkdownTagConverter(ILocalizer? localizer) : base(localizer)
		{
		}

		protected override string Convert(Tag tag, string @string)
		{
			if ((tag & Tag.Header) != 0)
			{
				@string = $"**{@string}**:";
			}
			if ((tag & Tag.Key) != 0)
			{
				@string = $"**{@string}** =";
			}
			if ((tag & Tag.Value) != 0)
			{
				@string = $"`{@string}`";
			}
			return @string;
		}
	}
}