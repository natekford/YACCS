using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Help.Models;

namespace YACCS.Help
{
	public class HelpFormatter : IHelpFormatter
	{
		protected Dictionary<IImmutableCommand, HelpCommand> Commands { get; }
			= new Dictionary<IImmutableCommand, HelpCommand>();
		protected ITypeRegistry<string> Names { get; }
		protected ITagConverter Tags { get; }

		public HelpFormatter(ITypeRegistry<string> names, ITagConverter tags)
		{
			Names = names;
			Tags = tags;
		}

		public ValueTask<string> FormatAsync(IContext context, IImmutableCommand command)
		{
			var help = GetHelpCommand(command);
			var builder = GetBuilder(context)
				.AppendNames(help)
				.AppendSummary(help);

			if (!help.IsAsyncFormattable())
			{
				builder
					.AppendAttributes(help)
					.AppendPreconditions(help)
					.AppendParameters(help);
				return new ValueTask<string>(builder.ToString());
			}

			static async Task<string> FormatAsync(HelpBuilder builder, IHelpCommand help)
			{
				if (help.HasAsyncFormattableAttributes)
				{
					await builder.AppendAttributesAsync(help).ConfigureAwait(false);
				}
				else
				{
					builder.AppendAttributes(help);
				}

				if (help.HasAsyncFormattablePreconditions)
				{
					await builder.AppendPreconditionsAsync(help).ConfigureAwait(false);
				}
				else
				{
					builder.AppendPreconditions(help);
				}

				if (help.HasAsyncFormattableParameters)
				{
					await builder.AppendParametersAsync(help).ConfigureAwait(false);
				}
				else
				{
					builder.AppendParameters(help);
				}

				return builder.ToString();
			}
			return new ValueTask<string>(FormatAsync(builder, help));
		}

		protected virtual HelpBuilder GetBuilder(IContext context)
			=> new HelpBuilder(context, Names, Tags);

		protected virtual IHelpCommand GetHelpCommand(IImmutableCommand command)
		{
			if (!Commands.TryGetValue(command, out var help))
			{
				Commands.Add(command, help = new HelpCommand(command));
			}
			return help;
		}

		protected class HelpBuilder
		{
			private static readonly TaggedString _TaggedAttributes
				= new TaggedString(Tag.Header, "Attributes");
			private static readonly TaggedString _TaggedNames
				= new TaggedString(Tag.Header, "Names");
			private static readonly TaggedString _TaggedParameters
				= new TaggedString(Tag.Header, "Parameters");
			private static readonly TaggedString _TaggedPreconditions
				= new TaggedString(Tag.Header, "Preconditions");
			private static readonly TaggedString _TaggedSummary
				= new TaggedString(Tag.Header, "Summary");

			protected IContext Context { get; }
			protected int CurrentDepth { get; set; }
			protected virtual string HeaderAttributes { get; }
			protected virtual string HeaderNames { get; }
			protected virtual string HeaderParameters { get; }
			protected virtual string HeaderPreconditions { get; }
			protected virtual string HeaderSummary { get; }
			protected ITypeRegistry<string> Names { get; }
			protected StringBuilder StringBuilder { get; }
			protected ITagConverter Tags { get; }

			public HelpBuilder(
				IContext context,
				ITypeRegistry<string> names,
				ITagConverter tags)
			{
				Context = context;
				Names = names;
				Tags = tags;

				HeaderAttributes = Tags.Convert(_TaggedAttributes);
				HeaderNames = Tags.Convert(_TaggedNames);
				HeaderParameters = Tags.Convert(_TaggedParameters);
				HeaderPreconditions = Tags.Convert(_TaggedPreconditions);
				HeaderSummary = Tags.Convert(_TaggedSummary);

				StringBuilder = new StringBuilder();
			}

			public HelpBuilder AppendAttributes(IHelpItem<object> item)
				=> AppendItems(HeaderAttributes, item.Attributes);

			public Task<HelpBuilder> AppendAttributesAsync(IHelpItem<object> item)
				=> AppendItemsAsync(HeaderAttributes, item.Attributes);

			public virtual HelpBuilder AppendItems(string header, IEnumerable<IHelpItem<object>> items)
			{
				var added = false;
				foreach (var item in items)
				{
					var text = Array.Empty<string>();
					if (item.Item is IRuntimeFormattableAttribute rfa)
					{
						text = Convert(rfa.Format(Context));
					}
					else if (item.Summary?.Summary is string summary)
					{
						text = new[] { summary };
					}
					AppendItemsText(ref added, header, text);
				}
				return this;
			}

			public virtual async Task<HelpBuilder> AppendItemsAsync(string header, IEnumerable<IHelpItem<object>> items)
			{
				var added = false;
				foreach (var item in items)
				{
					var text = Array.Empty<string>();
					if (item.Item is IAsyncRuntimeFormattableAttribute arfa)
					{
						text = Convert(await arfa.FormatAsync(Context).ConfigureAwait(false));
					}
					else if (item.Item is IRuntimeFormattableAttribute rfa)
					{
						text = Convert(rfa.Format(Context));
					}
					else if (item.Summary?.Summary is string summary)
					{
						text = new[] { summary };
					}
					AppendItemsText(ref added, header, text);
				}
				return this;
			}

			public virtual HelpBuilder AppendItemsText(ref bool added, string header, IReadOnlyList<string> text)
			{
				if (text is not null && text.Count != 0)
				{
					if (!added)
					{
						StringBuilder
							.AppendLine()
							.AppendDepth(CurrentDepth)
							.AppendLine(header);
						added = true;
					}

					StringBuilder.AppendDepth(CurrentDepth);
					foreach (var part in text)
					{
						StringBuilder.Append(part);
					}
					StringBuilder.AppendLine();
				}
				return this;
			}

			public virtual HelpBuilder AppendNames(IHelpCommand command)
			{
				StringBuilder
					.AppendDepth(CurrentDepth)
					.Append(HeaderNames)
					.AppendJoin(Tags.Separator, command.Item.Names)
					.AppendLine();
				return this;
			}

			public HelpBuilder AppendParameter(IHelpParameter parameter)
			{
				AppendType(parameter);
				++CurrentDepth;
				AppendSummary(parameter);
				AppendAttributes(parameter);
				AppendPreconditions(parameter);
				--CurrentDepth;
				return this;
			}

			public async Task<HelpBuilder> AppendParameterAsync(IHelpParameter parameter)
			{
				AppendType(parameter);
				++CurrentDepth;
				AppendSummary(parameter);
				if (parameter.HasAsyncFormattableAttributes)
				{
					await AppendAttributesAsync(parameter).ConfigureAwait(false);
				}
				else
				{
					AppendAttributes(parameter);
				}
				if (parameter.HasAsyncFormattablePreconditions)
				{
					await AppendPreconditionsAsync(parameter).ConfigureAwait(false);
				}
				else
				{
					AppendPreconditions(parameter);
				}
				--CurrentDepth;
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
					if (parameter.IsAsyncFormattable())
					{
						await AppendParameterAsync(parameter).ConfigureAwait(false);
					}
					else
					{
						AppendParameter(parameter);
					}
				}
				return this;
			}

			public virtual HelpBuilder AppendParametersHeader(ref bool added)
			{
				StringBuilder.AppendDepth(CurrentDepth);
				if (!added)
				{
					StringBuilder.AppendLine().AppendLine(HeaderParameters);
					added = true;
				}
				else
				{
					StringBuilder.AppendLine();
				}
				return this;
			}

			public HelpBuilder AppendPreconditions(IHasPreconditions preconditions)
				=> AppendItems(HeaderPreconditions, preconditions.Preconditions);

			public Task<HelpBuilder> AppendPreconditionsAsync(IHasPreconditions preconditions)
				=> AppendItemsAsync(HeaderPreconditions, preconditions.Preconditions);

			public virtual HelpBuilder AppendSummary(IHelpItem<object> item)
			{
				if (item.Summary is not null)
				{
					StringBuilder
						.AppendDepth(CurrentDepth)
						.Append(HeaderSummary)
						.AppendLine(item.Summary?.Summary);
				}
				return this;
			}

			public virtual HelpBuilder AppendType(IHelpParameter parameter)
			{
				var pType = parameter.ParameterType;
				StringBuilder
					.AppendDepth(CurrentDepth)
					.Append(parameter.Item.OverriddenParameterName)
					.Append(": ")
					.AppendLine(pType.Name?.Name ?? Names.Get(pType.Item));
				return this;
			}

			public string[] Convert(IReadOnlyList<TaggedString> tagged)
			{
				var untagged = new string[tagged.Count];
				for (var i = 0; i < tagged.Count; ++i)
				{
					untagged[i] = Tags.Convert(tagged[i]);
				}
				return untagged;
			}

			public override string ToString()
				=> StringBuilder.ToString();
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