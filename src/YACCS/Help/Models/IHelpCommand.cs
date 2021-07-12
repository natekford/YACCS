using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	public interface IHelpCommand : IHelpItem<IImmutableCommand>
	{
		IHelpItem<Type> ContextType { get; }
		IReadOnlyList<IHelpParameter> Parameters { get; }
		IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IPrecondition>>> Preconditions { get; }
	}
}