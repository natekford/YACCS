using System.Collections.Generic;

namespace YACCS.Help.Models
{
	public interface IHasPreconditions
	{
		IReadOnlyList<IHelpItem<object>> Preconditions { get; }
	}
}