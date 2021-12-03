namespace YACCS.Help;

/// <summary>
/// An implementation of <see cref="TagFormatter"/> that will print out in markdown.
/// </summary>
public class MarkdownTagFormatter : TagFormatter
{
	/// <inheritdoc />
	protected override Dictionary<string, Func<string, string>> Formatters { get; } = new(StringComparer.OrdinalIgnoreCase)
	{
		[Tag.Header] = x => $"**{x}**:",
		[Tag.Key] = x => $"**{x}** =",
		[Tag.Value] = x => $"`{x}`",
	};
}