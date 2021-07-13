using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.Localization;
using YACCS.Preconditions;

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
				.AppendSummary(help);

			await builder.AppendAttributesAsync(help).ConfigureAwait(false);
			await builder.AppendPreconditionsAsync(help.Preconditions).ConfigureAwait(false);
			await builder.AppendParametersAsync(help).ConfigureAwait(false);
			return builder.ToString();
		}

		public virtual IHelpCommand GetHelpCommand(IImmutableCommand command)
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

		protected virtual HelpBuilder GetBuilder(IContext context)
			=> new(context, TypeNames, FormatProvider);

		protected class HelpBuilder
		{
			protected IContext Context { get; }
			protected int CurrentDepth { get; set; }
			protected IFormatProvider? FormatProvider { get; }
			protected virtual string HeaderAttributes
				=> FormatProvider.Format(ToHeader(Keys.Attributes));
			protected virtual string HeaderNames
				=> FormatProvider.Format(ToHeader(Keys.Names));
			protected virtual string HeaderParameters
				=> FormatProvider.Format(ToHeader(Keys.Parameters));
			protected virtual string HeaderPreconditions
				=> FormatProvider.Format(ToHeader(Keys.Preconditions));
			protected virtual string HeaderSummary
				=> FormatProvider.Format(ToHeader(Keys.Summary));
			protected StringBuilder StringBuilder { get; }
			protected IReadOnlyDictionary<Type, string> TypeNames { get; }

			public HelpBuilder(
				IContext context,
				IReadOnlyDictionary<Type, string> typeNames,
				IFormatProvider? formatProvider)
			{
				Context = context;
				TypeNames = typeNames;
				FormatProvider = formatProvider;

				StringBuilder = new();
			}

			public virtual Task<HelpBuilder> AppendAttributesAsync(IHelpItem<object> item)
				=> AppendItemsAsync(HeaderAttributes, item.Attributes);

			public virtual HelpBuilder AppendNames(IHelpCommand command)
			{
				if (command.Item.Names.Count == 0)
				{
					return this;
				}

				Append(HeaderNames);
				var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
				StringBuilder.AppendJoin(separator, command.Item.Names);
				AppendLine();
				return this;
			}

			public virtual async Task<HelpBuilder> AppendParametersAsync(IHelpCommand command)
			{
				if (command.Parameters.Count == 0)
				{
					return this;
				}

				AppendLine(HeaderParameters);
				++CurrentDepth;
				foreach (var parameter in command.Parameters)
				{
					var pType = parameter.ParameterType;
					var typeName = pType.Name?.Name ?? TypeNames[pType.Item];
					Append(parameter.Item.ParameterName);
					StringBuilder.Append(": ");
					AppendLine(typeName);

					AppendSummary(parameter);
					await AppendAttributesAsync(parameter).ConfigureAwait(false);
					await AppendPreconditionsAsync(parameter.Preconditions).ConfigureAwait(false);
				}
				--CurrentDepth;
				return this;
			}

			public virtual async Task<HelpBuilder> AppendPreconditionsAsync<T>(
				IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<T>>> preconditions)
				where T : notnull
			{
				if (preconditions.Count == 0)
				{
					return this;
				}

				AppendLine(HeaderPreconditions);
				++CurrentDepth;
				foreach (var (group, lookup) in preconditions)
				{
					var groupHeader = group;
					if (!string.IsNullOrWhiteSpace(groupHeader))
					{
						groupHeader = FormatProvider.Format(ToHeader(groupHeader));
					}

					foreach (var items in lookup)
					{
						var opHeader = items.Key.ToString();
						await AppendItemsAsync(opHeader, items).ConfigureAwait(false);
					}
				}
				--CurrentDepth;
				return this;
			}

			public virtual HelpBuilder AppendSummary(IHelpItem<object> item)
			{
				if (item.Summary is null)
				{
					return this;
				}

				Append(HeaderSummary);
				AppendLine(item.Summary?.Summary);
				AppendLine();
				return this;
			}

			public override string ToString()
				=> StringBuilder.ToString();

			protected virtual HelpBuilder Append(string value)
			{
				AppendDepth(value);
				StringBuilder.Append(value);
				return this;
			}

			protected virtual HelpBuilder AppendDepth(string? value)
			{
				if (value is not null && (StringBuilder.Length == 0 || StringBuilder[^1] == '\n'))
				{
					StringBuilder.Append('\t', CurrentDepth);
				}
				return this;
			}

			protected virtual async Task<HelpBuilder> AppendItemsAsync<T>(
				string header,
				IEnumerable<IHelpItem<T>> items)
				where T : notnull
			{
				var hasHeader = false;
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
					else
					{
						continue;
					}

					if (!hasHeader)
					{
						if (!string.IsNullOrWhiteSpace(header))
						{
							AppendLine(header);
						}
						hasHeader = true;
					}

					AppendLine(text);
				}
				if (hasHeader)
				{
					AppendLine();
				}
				return this;
			}

			protected virtual HelpBuilder AppendLine(string? value = null)
			{
				AppendDepth(value);
				StringBuilder.AppendLine(value);
				return this;
			}

			private static FormattableString ToHeader(string value)
				=> $"{value:header} ";
		}
	}
}