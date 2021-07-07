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
		protected IReadOnlyDictionary<Type, string> TypeNames { get; }

		public HelpFormatter(
			IReadOnlyDictionary<Type, string> typeNames,
			IFormatProvider? formatProvider = null)
		{
			TypeNames = typeNames;
			FormatProvider = formatProvider;
		}

		public async ValueTask<string> FormatAsync(IContext context, IImmutableCommand command)
		{
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
			=> new(context, TypeNames, FormatProvider);

		protected virtual IHelpCommand GetHelpCommand(IImmutableCommand command)
		{
			while (command.Source != null)
			{
				command = command.Source;
			}

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
			protected virtual string HeaderAttributes
				=> FormatProvider.Format($"{Keys.Attributes:header} ");
			protected virtual string HeaderNames
				=> FormatProvider.Format($"{Keys.Names:header} ");
			protected virtual string HeaderParameters
				=> FormatProvider.Format($"{Keys.Parameters:header} ");
			protected virtual string HeaderPreconditions
				=> FormatProvider.Format($"{Keys.Preconditions:header} ");
			protected virtual string HeaderSummary
				=> FormatProvider.Format($"{Keys.Summary:header} ");
			protected StringBuilder StringBuilder { get; }
			protected IReadOnlyDictionary<Type, string> TypeNames { get; }

			public HelpBuilder(
				IContext context,
				IReadOnlyDictionary<Type, string> typeNames,
				IFormatProvider? formatProvider = null)
			{
				Context = context;
				TypeNames = typeNames;
				FormatProvider = formatProvider;

				StringBuilder = new();
			}

			public virtual Task<HelpBuilder> AppendAttributesAsync(IHelpItem<object> item)
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

			public virtual async Task<HelpBuilder> AppendParameterAsync(IHelpParameter parameter)
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

			public virtual async Task<HelpBuilder> AppendParametersAsync(IHelpCommand command)
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

			public virtual Task<HelpBuilder> AppendPreconditionsAsync(IHasPreconditions preconditions)
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
					.AppendLine(pType.Name?.Name ?? TypeNames[pType.Item]);
				return this;
			}

			public override string ToString()
				=> StringBuilder.ToString();
		}
	}
}