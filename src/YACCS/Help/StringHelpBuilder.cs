using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Help.Attributes;
using YACCS.Help.Models;
using YACCS.Localization;
using YACCS.Preconditions;

namespace YACCS.Help;

/// <summary>
/// Creates a string for information about a command.
/// </summary>
/// <param name="context">
/// <inheritdoc cref="Context" path="/summary"/>
/// </param>
/// <param name="typeNames">
/// <inheritdoc cref="TypeNames" path="/summary"/>
/// </param>
/// <param name="formatProvider">
/// <inheritdoc cref="FormatProvider" path="/summary"/>
/// </param>
public class StringHelpBuilder(
	IContext context,
	IReadOnlyDictionary<Type, string> typeNames,
	IFormatProvider? formatProvider
)
{
	/// <summary>
	/// The context invoking this help command.
	/// </summary>
	protected IContext Context { get; } = context;
	/// <summary>
	/// The current amount of tabs to append after each new line.
	/// </summary>
	protected int CurrentDepth { get; set; }
	/// <inheritdoc cref="StringHelpFactory.FormatProvider"/>
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
	/// <inheritdoc cref="StringHelpFactory.TypeNames"/>
	protected IReadOnlyDictionary<Type, string> TypeNames { get; } = typeNames;

	/// <inheritdoc />
	public virtual Task AddAttributesAsync(IReadOnlyList<HelpItem<object>> attributes)
		=> AddItemsAsync(HeaderAttributes, attributes);

	/// <inheritdoc />
	public virtual void AddNames(IReadOnlyList<IReadOnlyList<string>> paths)
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

	/// <inheritdoc />
	public virtual async Task AddParametersAsync(IReadOnlyList<HelpParameter> parameters)
	{
		if (parameters.Count == 0)
		{
			return;
		}

		using var _ = AppendHeader(HeaderParameters);
		++CurrentDepth;
		foreach (var parameter in parameters)
		{
			var pType = parameter.ParameterType;
			var typeName = pType.Name?.Name ?? TypeNames[pType.Item];
			Append(parameter.Item.ParameterName?.Name ?? parameter.Item.OriginalParameterName);
			StringBuilder.Append(": ");
			AppendLine(typeName);

			AddSummary(parameter.Summary);
			await AddAttributesAsync(parameter.Attributes).ConfigureAwait(false);
			await AddPreconditionsAsync(parameter.Preconditions).ConfigureAwait(false);

			if (parameter.NamedArguments is not null)
			{
				await AddParametersAsync(parameter.NamedArguments).ConfigureAwait(false);
			}
		}
		--CurrentDepth;
	}

	/// <inheritdoc />
	public virtual async Task AddPreconditionsAsync<T>(
		IReadOnlyDictionary<string, ILookup<Op, HelpItem<T>>> preconditions)
		where T : notnull
	{
		if (preconditions.Count == 0)
		{
			return;
		}

		using var _ = AppendHeader(HeaderPreconditions);
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
				await AddItemsAsync(opHeader, items).ConfigureAwait(false);
			}
		}
		--CurrentDepth;
	}

	/// <inheritdoc />
	public virtual void AddSummary(ISummaryAttribute? summary)
	{
		if (summary?.Summary is not string summaryString
			|| string.IsNullOrWhiteSpace(summaryString))
		{
			return;
		}

		Append(HeaderSummary);
		AppendLine(summaryString);
		AppendLine();
	}

	/// <inheritdoc />
	public override string ToString()
		=> StringBuilder.ToString();

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
	protected virtual async Task AddItemsAsync<T>(
		string header,
		IEnumerable<HelpItem<T>> items)
		where T : notnull
	{
		using var disposableHeader = AppendHeader(header);
		foreach (var item in items)
		{
			var text = default(string?);
			if (item.Item is ISummarizableAttribute summarizable)
			{
				text = await summarizable.GetSummaryAsync(Context, FormatProvider).ConfigureAwait(false);
			}
			if (string.IsNullOrWhiteSpace(text) && item.Summary?.Summary is string summary)
			{
				text = summary;
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}

			AppendLine(text);
		}
		if (!disposableHeader.IsValueEmpty)
		{
			AppendLine();
		}
	}

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
		if (!string.IsNullOrWhiteSpace(value) && (StringBuilder.Length == 0 || StringBuilder[^1] == '\n'))
		{
			StringBuilder.Append('\t', CurrentDepth);
		}
	}

	/// <summary>
	/// Appends a header then removes it if no value was provided.
	/// </summary>
	/// <param name="header"></param>
	/// <returns></returns>
	protected virtual DisposableHeader AppendHeader(string header)
	{
		var beforeHeader = StringBuilder.Length;
		AppendLine(header);
		var afterHeader = StringBuilder.Length;
		return new DisposableHeader(StringBuilder, beforeHeader, afterHeader);
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

	/// <summary>
	/// Append a header then remove it if no value was supplied.
	/// </summary>
	/// <param name="StringBuilder">The output.</param>
	/// <param name="BeforeHeader">The output's length before the header.</param>
	/// <param name="AfterHeader">The output's length after the header.</param>
	protected sealed class DisposableHeader(
		StringBuilder StringBuilder,
		int BeforeHeader,
		int AfterHeader
	) : IDisposable
	{
		/// <summary>
		/// Whether or not a value was appended after this header.
		/// </summary>
		public bool IsValueEmpty => StringBuilder.Length == AfterHeader;

		/// <inheritdoc />
		public void Dispose()
		{
			if (IsValueEmpty)
			{
				StringBuilder.Length = BeforeHeader;
			}
		}
	}
}