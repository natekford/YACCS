using System.Collections.Generic;
using System.Globalization;
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

		public HelpFormatter(ITypeRegistry<string> names)
		{
			_Names = names;
		}

		public async Task<string> FormatAsync(IContext context, HelpCommand command)
		{
			var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
			var sb = new StringBuilder();

			// Names
			{
				sb.Append("Names: ")
					.AppendJoin(separator, command.Item.Names)
					.AppendLine();
			}

			// Summary
			{
				if (command.Summary != null)
				{
					sb.Append("Summary: ")
						.AppendLine(command.Summary?.Summary);
				}
			}

			// Attributes
			{
				var attributes = command.Attributes;
				if (attributes.Count > 0)
				{
					await AppendItemsAsync(context, separator, "Attributes:", sb, attributes).ConfigureAwait(false);
				}
			}

			// Preconditions
			{
				var preconditions = command.Preconditions;
				if (preconditions.Count > 0)
				{
					await AppendItemsAsync(context, separator, "Preconditions:", sb, preconditions).ConfigureAwait(false);
				}
			}

			// Parameters
			{
				for (var i = 0; i < command.Parameters.Count; ++i)
				{
					var parameter = command.Parameters[i];
					if (i == 0)
					{
						sb.AppendLine().AppendLine("Parameters:");
					}
					else
					{
						sb.AppendLine();
					}

					await AppendParameterAsync(context, separator, sb, parameter).ConfigureAwait(false);
				}
				if (command.Parameters.Count > 0)
				{
					sb.AppendLine();
				}
			}

			return sb.ToString();
		}

		private async Task AppendItemsAsync(
			IContext context,
			string separator,
			string header,
			StringBuilder sb,
			IReadOnlyList<IHelpItem<object>> items)
		{
			var added = false;
			foreach (var item in items)
			{
				string? text;
				if (item.Item is IRuntimeFormattableAttribute rfa)
				{
					text = await rfa.FormatAsync(context).ConfigureAwait(false);
				}
				else if (item.Summary?.Summary is string summary)
				{
					text = summary;
				}
				else
				{
					continue;
				}

				if (!added)
				{
					sb.AppendLine().AppendLine(header);
					added = true;
				}
				else
				{
					sb.Append(separator);
				}
				sb.Append(text);
			}
			if (items.Count > 0)
			{
				sb.AppendLine();
			}
		}

		private async Task AppendParameterAsync(
			IContext context,
			string separator,
			StringBuilder sb,
			IHelpParameter parameter)
		{
			// Names
			{
				sb.Append("Name: ").AppendLine(parameter.Item.OverriddenParameterName);
			}

			// Summary
			{
				if (parameter.Summary != null)
				{
					sb.Append("Summary: ")
						.AppendLine(parameter.Summary?.Summary);
				}
			}

			// Attributes
			{
				var attributes = parameter.Attributes;
				if (attributes.Count > 0)
				{
					await AppendItemsAsync(context, separator, "Attributes:", sb, attributes).ConfigureAwait(false);
				}
			}

			// Preconditions
			{
				var preconditions = parameter.Preconditions;
				if (preconditions.Count > 0)
				{
					await AppendItemsAsync(context, separator, "Preconditions:", sb, preconditions).ConfigureAwait(false);
				}
			}
		}
	}
}