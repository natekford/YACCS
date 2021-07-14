using System;
using System.Collections.Generic;

namespace YACCS.Help
{
	public class MarkdownTagFormatter : TagFormatter
	{
		protected override Dictionary<string, Func<string, string>> Formatters { get; } = new(StringComparer.OrdinalIgnoreCase)
		{
			[Tag.Header] = x => $"**{x}**:",
			[Tag.Key] = x => $"**{x}** =",
			[Tag.Value] = x => $"`{x}`",
		};
	}
}