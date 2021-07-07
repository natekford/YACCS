namespace YACCS.Help
{
	public class MarkdownTagConverter : TagConverter
	{
		public MarkdownTagConverter()
		{
			Formatters[Tag.Header] = x => $"**{x}**:";
			Formatters[Tag.Key] = x => $"**{x}** =";
			Formatters[Tag.Value] = x => $"`{x}`";
		}
	}
}