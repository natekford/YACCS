using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Help.Attributes;
using YACCS.Help.Models;

namespace YACCS.Help
{
	public class HelpFormatter : IHelpFormatter
	{
		private readonly ITypeRegistry<string> _Names;
		private readonly ITagConverter _Tags;

		public HelpFormatter(ITypeRegistry<string> names, ITagConverter tags)
		{
			_Names = names;
			_Tags = tags;
		}

		public ValueTask<string> FormatAsync(IContext context, IHelpCommand command)
		{
			var builder = new HelpBuilder(context, _Names, _Tags)
				.AppendNames(command)
				.AppendSummary(command);

			if (command.IsAsyncFormattable)
			{
				static async Task<string> FormatAsync(HelpBuilder builder, IHelpCommand command)
				{
					await builder.AppendAttributesAsync(command).ConfigureAwait(false);
					await builder.AppendPreconditionsAsync(command).ConfigureAwait(false);
					await builder.AppendParametersAsync(command).ConfigureAwait(false);
					return builder.StringBuilder.ToString();
				}
				return new ValueTask<string>(FormatAsync(builder, command));
			}
			else
			{
				builder
					.AppendAttributes(command)
					.AppendPreconditions(command)
					.AppendParameters(command);
				return new ValueTask<string>(builder.StringBuilder.ToString());
			}
		}

		private sealed class HelpBuilder
		{
			private const string ATTRIBUTES = "Attributes: ";
			private const string NAMES = "Names: ";
			private const string PARAMETERS = "Parameters: ";
			private const string PRECONDITIONS = "Preconditions: ";
			private const string SUMMARY = "Summary: ";

			private readonly IContext _Context;
			private readonly ITypeRegistry<string> _Names;
			private readonly string _Separator;
			private readonly ITagConverter _Tags;
			private int _CurrentDepth;

			public StringBuilder StringBuilder { get; }

			public HelpBuilder(IContext context, ITypeRegistry<string> names, ITagConverter tags)
			{
				_Context = context;
				_Names = names;
				_Tags = tags;
				_Separator = tags.Separator;
				StringBuilder = new StringBuilder();
			}

			public HelpBuilder AppendAttributes(IHelpItem<object> item)
				=> AppendItems(ATTRIBUTES, item.Attributes);

			public Task<HelpBuilder> AppendAttributesAsync(IHelpItem<object> item)
				=> AppendItemsAsync(ATTRIBUTES, item.Attributes);

			public HelpBuilder AppendItems(string header, IEnumerable<IHelpItem<object>> items)
			{
				var added = false;
				foreach (var item in items)
				{
					var text = Array.Empty<string>();
					if (item.Item is IRuntimeFormattableAttribute rfa)
					{
						text = Convert(rfa.Format(_Context));
					}
					else if (item.Summary?.Summary is string summary)
					{
						text = new[] { summary };
					}
					AppendItemsText(ref added, header, text);
				}
				return this;
			}

			public async Task<HelpBuilder> AppendItemsAsync(string header, IEnumerable<IHelpItem<object>> items)
			{
				var added = false;
				foreach (var item in items)
				{
					var text = Array.Empty<string>();
					if (item.Item is IAsyncRuntimeFormattableAttribute arfa)
					{
						text = Convert(await arfa.FormatAsync(_Context).ConfigureAwait(false));
					}
					else if (item.Item is IRuntimeFormattableAttribute rfa)
					{
						text = Convert(rfa.Format(_Context));
					}
					else if (item.Summary?.Summary is string summary)
					{
						text = new[] { summary };
					}
					AppendItemsText(ref added, header, text);
				}
				return this;
			}

			public HelpBuilder AppendItemsText(ref bool added, string header, IReadOnlyList<string> text)
			{
				if (text is not null && text.Count != 0)
				{
					if (!added)
					{
						StringBuilder
							.AppendLine()
							.AppendDepth(_CurrentDepth)
							.AppendLine(header);
						added = true;
					}

					StringBuilder.AppendDepth(_CurrentDepth);
					foreach (var part in text)
					{
						StringBuilder.Append(part);
					}
					StringBuilder.AppendLine();
				}
				return this;
			}

			public HelpBuilder AppendNames(IHelpCommand command)
			{
				StringBuilder
					.AppendDepth(_CurrentDepth)
					.Append(NAMES)
					.AppendJoin(_Separator, command.Item.Names)
					.AppendLine();
				return this;
			}

			public HelpBuilder AppendParameter(IHelpParameter parameter)
			{
				AppendType(parameter);
				++_CurrentDepth;
				AppendSummary(parameter);
				AppendAttributes(parameter);
				AppendPreconditions(parameter);
				--_CurrentDepth;
				return this;
			}

			public async Task<HelpBuilder> AppendParameterAsync(IHelpParameter parameter)
			{
				AppendType(parameter);
				++_CurrentDepth;
				AppendSummary(parameter);
				await AppendAttributesAsync(parameter).ConfigureAwait(false);
				await AppendPreconditionsAsync(parameter).ConfigureAwait(false);
				--_CurrentDepth;
				return this;
			}

			public HelpBuilder AppendParameters(IHelpCommand command)
			{
				var added = false;
				foreach (var parameter in command.Parameters)
				{
					AppendParametersHeader(ref added);
					AppendParameter(parameter);
				}
				return this;
			}

			public async Task<HelpBuilder> AppendParametersAsync(IHelpCommand command)
			{
				var added = false;
				foreach (var parameter in command.Parameters)
				{
					AppendParametersHeader(ref added);
					await AppendParameterAsync(parameter).ConfigureAwait(false);
				}
				return this;
			}

			public HelpBuilder AppendParametersHeader(ref bool added)
			{
				StringBuilder.AppendDepth(_CurrentDepth);
				if (!added)
				{
					StringBuilder.AppendLine().AppendLine(PARAMETERS);
					added = true;
				}
				else
				{
					StringBuilder.AppendLine();
				}
				return this;
			}

			public HelpBuilder AppendPreconditions(IHasPreconditions preconditions)
				=> AppendItems(PRECONDITIONS, preconditions.Preconditions);

			public Task<HelpBuilder> AppendPreconditionsAsync(IHasPreconditions preconditions)
				=> AppendItemsAsync(PRECONDITIONS, preconditions.Preconditions);

			public HelpBuilder AppendSummary(IHelpItem<object> item)
			{
				if (item.Summary is not null)
				{
					StringBuilder
						.AppendDepth(_CurrentDepth)
						.Append(SUMMARY)
						.AppendLine(item.Summary?.Summary);
				}
				return this;
			}

			public HelpBuilder AppendType(IHelpParameter parameter)
			{
				var pType = parameter.ParameterType;
				StringBuilder
					.AppendDepth(_CurrentDepth)
					.Append(parameter.Item.OverriddenParameterName)
					.Append(": ")
					.AppendLine(pType.Name?.Name ?? _Names.Get(pType.Item));
				return this;
			}

			public string[] Convert(IReadOnlyList<TaggedString> tagged)
			{
				var untagged = new string[tagged.Count];
				for (var i = 0; i < tagged.Count; ++i)
				{
					untagged[i] = _Tags.Convert(tagged[i]);
				}
				return untagged;
			}
		}
	}

	internal static class HelpBuilderUtils
	{
		public static StringBuilder AppendDepth(this StringBuilder sb, int depth)
		{
			for (var i = 0; i < depth; ++i)
			{
				sb.Append('\t');
			}
			return sb;
		}
	}
}