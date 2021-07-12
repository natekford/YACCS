using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class HelpItem<T> : IHelpItem<T> where T : notnull
	{
		public IReadOnlyList<IHelpItem<object>> Attributes { get; }
		public T Item { get; }
		public INameAttribute? Name { get; }
		public ISummaryAttribute? Summary { get; }
		private string DebuggerDisplay => $"Type = {Item.GetType()}, Attribute Count = {Attributes.Count}";

		public HelpItem(T item)
			: this(item, item.GetType().GetCustomAttributes(true))
		{
		}

		public HelpItem(T item, IReadOnlyList<object> attributes)
			: this(item, attributes, _ => true)
		{
		}

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
}