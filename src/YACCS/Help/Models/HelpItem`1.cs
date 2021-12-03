using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models;

/// <inheritdoc cref="IHelpItem{T}"/>
[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
public class HelpItem<T> : IHelpItem<T> where T : notnull
{
	/// <inheritdoc />
	public IReadOnlyList<IHelpItem<object>> Attributes { get; }
	/// <inheritdoc />
	public T Item { get; }
	/// <inheritdoc />
	public INameAttribute? Name { get; }
	/// <inheritdoc />
	public ISummaryAttribute? Summary { get; }
	private string DebuggerDisplay => $"Type = {Item.GetType()}, Attribute Count = {Attributes.Count}";

	/// <inheritdoc cref="HelpItem(T, IReadOnlyList{object}, Func{object, bool})"/>
	public HelpItem(T item)
		: this(item, item.GetType().GetCustomAttributes(true))
	{
	}

	/// <inheritdoc cref="HelpItem(T, IReadOnlyList{object}, Func{object, bool})"/>
	public HelpItem(T item, IReadOnlyList<object> attributes)
		: this(item, attributes, _ => true)
	{
	}

	/// <summary>
	/// Creates a new <see cref="HelpItem{T}"/>.
	/// </summary>
	/// <param name="item">The item to present information about.</param>
	/// <param name="attributes">The attributes for this item.</param>
	/// <param name="allowedAttributes">The filter to use for these attributes.</param>
	public HelpItem(T item, IReadOnlyList<object> attributes, Func<object, bool> allowedAttributes)
	{
		Item = item;

		var items = new List<IHelpItem<object>>();
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

	internal HelpItem<T2> Create<T2>(T2 item) where T2 : ICustomAttributeProvider
		=> new(item, item.GetCustomAttributes(true));
}
