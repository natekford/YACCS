using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	public class HelpCommand : HelpItem<IImmutableCommand>, IHelpCommand
	{
		public IHelpItem<Type>? ContextType { get; }
		public IReadOnlyList<IHelpParameter> Parameters { get; }
		public IReadOnlyList<IHelpItem<IPrecondition>> Preconditions { get; }

		public HelpCommand(IImmutableCommand item)
			: base(item, item.Attributes, x => x is not IPrecondition)
		{
			ContextType = item.ContextType is Type c ? new HelpType(c) : null;
			Parameters = item.Parameters
				.Select(x => new HelpParameter(x))
				.ToImmutableArray();
			Preconditions = item.Preconditions
				.Select(x => new HelpItem<IPrecondition>(x))
				.ToImmutableArray();
		}
	}
}