namespace YACCS.Help
{
	public class MarkdownTagFormatter : TagFormatter
	{
		public MarkdownTagFormatter()
		{
			Formatters[Tag.Header] = x => $"**{x}**:";
			Formatters[Tag.Key] = x => $"**{x}** =";
			Formatters[Tag.Value] = x => $"`{x}`";
		}
	}
}