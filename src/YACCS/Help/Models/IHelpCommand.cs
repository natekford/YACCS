using System;
using System.Collections.Generic;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	public interface IHelpCommand : IHelpItem<IImmutableCommand>, IHasPreconditions
	{
		IHelpItem<Type>? ContextType { get; }
		bool HasAsyncFormattableParameters { get; }
		IReadOnlyList<IHelpParameter> Parameters { get; }
		new IReadOnlyList<IHelpItem<IPrecondition>> Preconditions { get; }
	}
}