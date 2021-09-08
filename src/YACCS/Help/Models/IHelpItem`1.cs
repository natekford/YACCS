using System.Collections.Generic;

using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models
{
	/// <summary>
	/// Information to display about an item in a help command.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IHelpItem<out T> where T : notnull
	{
		/// <summary>
		/// The attributes associated with this item.
		/// </summary>
		IReadOnlyList<IHelpItem<object>> Attributes { get; }
		/// <summary>
		/// The item to present information about.
		/// </summary>
		T Item { get; }
		/// <summary>
		/// The name of this item.
		/// </summary>
		INameAttribute? Name { get; }
		/// <summary>
		/// The summary of this item.
		/// </summary>
		ISummaryAttribute? Summary { get; }
	}
}