using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.Localization;
using YACCS.Preconditions;

namespace YACCS.Help;

/// <summary>
/// Formats a command to a string.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="HelpFormatter"/>.
/// </remarks>
/// <param name="typeNames">
/// <inheritdoc cref="TypeNames" path="/summary"/>
/// </param>
/// <param name="formatProvider">
/// <inheritdoc cref="FormatProvider" path="/summary"/>
/// </param>
public class HelpFormatter(
	IReadOnlyDictionary<Type, string> typeNames,
	IFormatProvider? formatProvider = null)
	: IHelpFormatter
{
	private readonly ConcurrentDictionary<IImmutableCommand, HelpCommand> _Commands = [];
	/// <summary>
	/// The format provider to use when formatting strings.
	/// </summary>
	protected IFormatProvider? FormatProvider { get; } = formatProvider;
	/// <summary>
	/// Type names to use for displaying types.
	/// </summary>
	protected IReadOnlyDictionary<Type, string> TypeNames { get; } = typeNames;

	/// <inheritdoc />
	public async ValueTask<string> FormatAsync(IContext context, IImmutableCommand command)
	{
		var help = GetHelpCommand(command);
		var builder = GetBuilder(context);

		builder.AppendNames(help.Item.Paths);
		builder.AppendSummary(help.Summary);
		await builder.AppendAttributesAsync(help.Attributes).ConfigureAwait(false);
		await builder.AppendPreconditionsAsync(help.Preconditions).ConfigureAwait(false);
		await builder.AppendParametersAsync(help.Parameters).ConfigureAwait(false);
		return builder.ToString();
	}

	/// <summary>
	/// Creates a new <see cref="HelpBuilder"/>.
	/// </summary>
	/// <param name="context">The context invoking this help command.</param>
	/// <returns>A new help builder.</returns>
	protected virtual HelpBuilder GetBuilder(IContext context)
		=> new(context, TypeNames, FormatProvider);

	/// <summary>
	/// Creates a new <see cref="HelpCommand"/>.
	/// </summary>
	/// <param name="command">The command to create a help command for.</param>
	/// <returns>A new help command.</returns>
	protected virtual HelpCommand GetHelpCommand(IImmutableCommand command)
	{
		while (command.Source is not null)
		{
			command = command.Source;
		}

		return _Commands.GetOrAdd(command, k => new(k));
	}

	/// <summary>
	/// Builds a string that represents a command.
	/// </summary>
	/// <remarks>
	/// Creates a new <see cref="HelpBuilder"/>.
	/// </remarks>
	/// <param name="context">
	/// <inheritdoc cref="Context" path="/summary"/>
	/// </param>
	/// <param name="typeNames">
	/// <inheritdoc cref="TypeNames" path="/summary"/>
	/// </param>
	/// <param name="formatProvider">
	/// <inheritdoc cref="FormatProvider" path="/summary"/>
	/// </param>
	protected class HelpBuilder(
		IContext context,
		IReadOnlyDictionary<Type, string> typeNames,
		IFormatProvider? formatProvider)
	{
		/// <summary>
		/// The context invoking this help command.
		/// </summary>
		protected IContext Context { get; } = context;
		/// <summary>
		/// The current amount of tabs to append after each new line.
		/// </summary>
		protected int CurrentDepth { get; set; }
		/// <inheritdoc cref="HelpFormatter.FormatProvider"/>
		protected IFormatProvider? FormatProvider { get; } = formatProvider;
		/// <summary>
		/// The header for the attributes section.
		/// </summary>
		protected virtual string HeaderAttributes => ToHeader(Keys.Attributes);
		/// <summary>
		/// The header for the names section.
		/// </summary>
		protected virtual string HeaderNames => ToHeader(Keys.Names);
		/// <summary>
		/// The header for the parameters section.
		/// </summary>
		protected virtual string HeaderParameters => ToHeader(Keys.Parameters);
		/// <summary>
		/// The header for the preconditions section.
		/// </summary>
		protected virtual string HeaderPreconditions => ToHeader(Keys.Preconditions);
		/// <summary>
		/// The header for the summary section.
		/// </summary>
		protected virtual string HeaderSummary => ToHeader(Keys.Summary);
		/// <summary>
		/// The string builder creating the text representing this help command.
		/// </summary>
		protected StringBuilder StringBuilder { get; } = new();
		/// <inheritdoc cref="HelpFormatter.TypeNames"/>
		protected IReadOnlyDictionary<Type, string> TypeNames { get; } = typeNames;

		/// <summary>
		/// Appends attributes from <paramref name="attributes"/> to this instance.
		/// </summary>
		/// <param name="attributes">The attributes to append.</param>
		/// <returns></returns>
		public virtual Task AppendAttributesAsync(IReadOnlyList<HelpItem<object>> attributes)
			=> AppendItemsAsync(HeaderAttributes, attributes);

		/// <summary>
		/// Appends each name in <paramref name="paths"/> to this instance.
		/// </summary>
		/// <param name="paths">The paths to format and append.</param>
		public virtual void AppendNames(IReadOnlyList<IReadOnlyList<string>> paths)
		{
			if (paths.Count == 0)
			{
				return;
			}

			Append(HeaderNames);
			var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
			StringBuilder.AppendJoin(separator, paths);
			AppendLine();
		}

		/// <summary>
		/// Appends each parameter in <paramref name="parameters"/> to this instance.
		/// </summary>
		/// <param name="parameters">The parameters to format and append.</param>
		/// <returns></returns>
		public virtual async Task AppendParametersAsync(IReadOnlyList<HelpParameter> parameters)
		{
			if (parameters.Count == 0)
			{
				return;
			}

			AppendLine(HeaderParameters);
			++CurrentDepth;
			foreach (var parameter in parameters)
			{
				var pType = parameter.ParameterType;
				var typeName = pType.Name?.Name ?? TypeNames[pType.Item];
				Append(parameter.Item.ParameterName);
				StringBuilder.Append(": ");
				AppendLine(typeName);

				AppendSummary(parameter.Summary);
				await AppendAttributesAsync(parameter.Attributes).ConfigureAwait(false);
				await AppendPreconditionsAsync(parameter.Preconditions).ConfigureAwait(false);
			}
			--CurrentDepth;
		}

		/// <summary>
		/// Appends each group in <paramref name="preconditions"/> to this instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="preconditions">The preconditions to format and append.</param>
		/// <returns></returns>
		public virtual async Task AppendPreconditionsAsync<T>(
			IReadOnlyDictionary<string, ILookup<Op, HelpItem<T>>> preconditions)
			where T : notnull
		{
			if (preconditions.Count == 0)
			{
				return;
			}

			AppendLine(HeaderPreconditions);
			++CurrentDepth;
			foreach (var (group, lookup) in preconditions)
			{
				var groupHeader = group;
				if (!string.IsNullOrWhiteSpace(groupHeader))
				{
					groupHeader = ToHeader(groupHeader);
				}

				foreach (var items in lookup)
				{
					var opHeader = items.Key.ToString();
					await AppendItemsAsync(opHeader, items).ConfigureAwait(false);
				}
			}
			--CurrentDepth;
		}

		/// <summary>
		/// Appends <paramref name="summary"/> to this instance.
		/// </summary>
		/// <param name="summary">The summary to format and append.</param>
		public virtual void AppendSummary(ISummaryAttribute? summary)
		{
			if (summary is null)
			{
				return;
			}

			Append(HeaderSummary);
			AppendLine(summary.Summary);
			AppendLine();
		}

		/// <inheritdoc />
		public override string ToString()
			=> StringBuilder.ToString();

		/// <summary>
		/// Appends <paramref name="value"/> to this instance.
		/// </summary>
		/// <param name="value">The string to append.</param>
		protected virtual void Append(string value)
		{
			AppendDepth(value);
			StringBuilder.Append(value);
		}

		/// <summary>
		/// Appends <see cref="CurrentDepth"/> amount of tabs at the start of each
		/// new line.
		/// </summary>
		/// <param name="value">
		/// The string to make sure a value is actually going to be appended.
		/// </param>
		/// <remarks>
		/// If <paramref name="value"/> is null, this method does nothing.
		/// If <paramref name="value"/> is not null, this method will append tabs only
		/// if the previous character is a new line or this instance is empty.
		/// </remarks>
		protected virtual void AppendDepth(string? value)
		{
			if (value is not null && (StringBuilder.Length == 0 || StringBuilder[^1] == '\n'))
			{
				StringBuilder.Append('\t', CurrentDepth);
			}
		}

		/// <summary>
		/// Appends <paramref name="header"/> and then appends each item in
		/// <paramref name="items"/> to this instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="header">The header to append.</param>
		/// <param name="items">The items to format and append.</param>
		/// <returns></returns>
		/// <remarks>
		/// <paramref name="header"/> will not get appended if none of the items
		/// in <paramref name="items"/> can be formatted as a string.
		/// </remarks>
		protected virtual async Task AppendItemsAsync<T>(
			string header,
			IEnumerable<HelpItem<T>> items)
			where T : notnull
		{
			var shouldAppendHeader = false;
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

				if (!shouldAppendHeader)
				{
					if (!string.IsNullOrWhiteSpace(header))
					{
						AppendLine(header);
					}
					shouldAppendHeader = true;
				}

				AppendLine(text);
			}
			if (shouldAppendHeader)
			{
				AppendLine();
			}
		}

		/// <summary>
		/// Appends <paramref name="value"/> to this instance and then appends
		/// a new line.
		/// </summary>
		/// <param name="value">The string to append.</param>
		protected virtual void AppendLine(string? value = null)
		{
			AppendDepth(value);
			StringBuilder.AppendLine(value);
		}

		private string ToHeader(string value)
			=> FormatProvider.Format($"{value:header} ");

		private string ToHeader(NeedsLocalization key)
			=> ToHeader(key.Localized);
	}
}