using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models;

/// <summary>
/// Information to display about an item in a help command.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpItem<T> where T : notnull
{
	/// <summary>
	/// The attributes associated with this item.
	/// </summary>
	public virtual IReadOnlyList<HelpItem<object>> Attributes { get; }
	/// <summary>
	/// The item to present information about.
	/// </summary>
	public virtual T Item { get; }
	/// <summary>
	/// The name of this item.
	/// </summary>
	public virtual INameAttribute? Name { get; }
	/// <summary>
	/// The summary of this item.
	/// </summary>
	public virtual ISummaryAttribute? Summary { get; }
	private string DebuggerDisplay
		=> $"Type = {Item.GetType()}, Attribute Count = {Attributes.Count}";

	/// <inheritdoc cref="HelpItem(T, IEnumerable{object}, Func{object, bool})"/>
	public HelpItem(T item)
		: this(item, item.GetType().GetCustomAttributes(true))
	{
	}

	/// <inheritdoc cref="HelpItem(T, IEnumerable{object}, Func{object, bool})"/>
	public HelpItem(T item, IEnumerable<object> attributes)
		: this(item, attributes, _ => true)
	{
	}

	/// <summary>
	/// Creates a new <see cref="HelpItem{T}"/>.
	/// </summary>
	/// <param name="item">The item to present information about.</param>
	/// <param name="attributes">The attributes for this item.</param>
	/// <param name="allowedAttributes">The filter to use for these attributes.</param>
	public HelpItem(T item, IEnumerable<object> attributes, Func<object, bool> allowedAttributes)
	{
		Item = item;

		var items = new List<HelpItem<object>>();
		int n = 0, s = 0;
		foreach (var attribute in attributes)
		{
			// Don't allow attributes to keep adding themselves over and over
			if (allowedAttributes(attribute) && item.GetType() != attribute.GetType())
			{
				items.Add(new HelpItem<object>(attribute));
			}

			switch (attribute)
			{
				case ISummaryAttribute summary:
					Summary = summary.ThrowIfDuplicate(x => x, ref s);
					break;

				case INameAttribute name:
					Name = name.ThrowIfDuplicate(x => x, ref n);
					break;
			}
		}
		Attributes = items.ToImmutableArray();
	}
}