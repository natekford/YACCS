using System.Collections.Generic;

namespace YACCS.Help.Models
{
	public interface IHasPreconditions
	{
		bool HasAsyncFormattablePreconditions { get; }
		IReadOnlyList<IHelpItem<object>> Preconditions { get; }
	}
}