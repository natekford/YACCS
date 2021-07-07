using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.Localization;

namespace YACCS.Help
{
	public class HelpFormatter : IHelpFormatter
	{
		protected Dictionary<IImmutableCommand, HelpCommand> Commands { get; } = new();
		protected IFormatProvider? FormatProvider { get; }
		protected IReadOnlyDictionary<Type, string> Names { get; }

		public HelpFormatter(
			IReadOnlyDictionary<Type, string> names,
			IFormatProvider? formatProvider = null)
		{
			Names = names;
			FormatProvider = formatProvider;
		}

		public async ValueTask<string> FormatAsync(IContext context, IImmutableCommand command)
		{
			while (command.Source != null)
			{
				command = command.Source;
			}

			var help = GetHelpCommand(command);
			var builder = GetBuilder(context)
				.AppendNames(help)
				.AppendSummary(help)
				.AppendLine();

			await builder.AppendAttributesAsync(help).ConfigureAwait(false);
			await builder.AppendPreconditionsAsync(help).ConfigureAwait(false);
			await builder.AppendParametersAsync(help).ConfigureAwait(false);
			return new(builder.ToString());
		}

		protected virtual HelpBuilder GetBuilder(IContext context)
			=> new(context, Names, FormatProvider);

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
			protected IContext Context { get; }
			protected int CurrentDepth { get; set; }
			protected IFormatProvider? FormatProvider { get; }
			protected virtual string HeaderAttributes { get; }
			protected virtual string HeaderNames { get; }
			protected virtual string HeaderParameters { get; }
			protected virtual string HeaderPreconditions { get; }
			protected virtual string HeaderSummary { get; }
			protected IReadOnlyDictionary<Type, string> Names { get; }
			protected StringBuilder StringBuilder { get; }

			public HelpBuilder(
				IContext context,
				IReadOnlyDictionary<Type, string> names,
				IFormatProvider? formatProvider = null)
			{
				Context = context;
				Names = names;
				FormatProvider = formatProvider;

				HeaderAttributes = FormatProvider.Format($"{Keys.ATTRIBUTES:h} ");
				HeaderNames = FormatProvider.Format($"{Keys.NAMES:h} ");
				HeaderParameters = FormatProvider.Format($"{Keys.PARAMETERS:h} ");
				HeaderPreconditions = FormatProvider.Format($"{Keys.PRECONDITIONS:h} ");
				HeaderSummary = FormatProvider.Format($"{Keys.SUMMARY:h} ");

				StringBuilder = new();
			}

			public Task<HelpBuilder> AppendAttributesAsync(IHelpItem<object> item)
				=> AppendItemsAsync(HeaderAttributes, item.Attributes);

			public virtual async Task<HelpBuilder> AppendItemsAsync(
				string header,
				IReadOnlyList<IHelpItem<object>> items)
			{
				var added = false;
				foreach (var item in items)
				{
					var text = default(string?);
					if (item.Item is IRuntimeFormattableAttribute f)
					{
						text = await f.FormatAsync(Context, FormatProvider).ConfigureAwait(false);
					}
					else if (item.Summary?.Summary is string summary)
					{
						text = summary;
					}
					AppendItemsText(ref added, header, text);
				}
				if (added)
				{
					AppendLine();
				}
				return this;
			}

			public virtual HelpBuilder AppendItemsText(ref bool added, string header, string? text)
			{
				if (text is not null)
				{
					if (!added)
					{
						StringBuilder
							.AppendDepth(CurrentDepth)
							.AppendLine(header);
						added = true;
					}

					StringBuilder.AppendDepth(CurrentDepth);
					StringBuilder.AppendLine(text);
				}
				return this;
			}

			public virtual HelpBuilder AppendLine()
			{
				StringBuilder.AppendLine();
				return this;
			}

			public virtual HelpBuilder AppendNames(IHelpCommand command)
			{
				StringBuilder
					.AppendDepth(CurrentDepth)
					.Append(HeaderNames)
					.AppendJoin(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", command.Item.Names)
					.AppendLine();
				return this;
			}

			public async Task<HelpBuilder> AppendParameterAsync(IHelpParameter parameter)
			{
				AppendType(parameter);
				++CurrentDepth;
				AppendSummary(parameter);
				AppendLine();
				await AppendAttributesAsync(parameter).ConfigureAwait(false);
				await AppendPreconditionsAsync(parameter).ConfigureAwait(false);
				--CurrentDepth;
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

			public virtual HelpBuilder AppendParametersHeader(ref bool added)
			{
				StringBuilder.AppendDepth(CurrentDepth);
				if (!added)
				{
					StringBuilder.AppendLine(HeaderParameters);
					added = true;
				}
				else
				{
					StringBuilder.AppendLine();
				}
				return this;
			}

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
					.Append(parameter.Item.ParameterName)
					.Append(": ")
					.AppendLine(pType.Name?.Name ?? Names[pType.Item]);
				return this;
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