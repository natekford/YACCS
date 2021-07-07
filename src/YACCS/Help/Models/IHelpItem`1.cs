using System.Collections.Generic;

using YACCS.Commands.Attributes;
using YACCS.Help.Attributes;

namespace YACCS.Help.Models
{
	public interface IHelpItem<out T> where T : notnull
	{
		IReadOnlyList<IHelpItem<object>> Attributes { get; }
		T Item { get; }
		INameAttribute? Name { get; }
		ISummaryAttribute? Summary { get; }
	}
}