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
		public override bool IsAsyncFormattable { get; }
		public IReadOnlyList<IHelpParameter> Parameters { get; }
		public IReadOnlyList<IHelpItem<IPrecondition>> Preconditions { get; }
		IReadOnlyList<IHelpItem<object>> IHasPreconditions.Preconditions => Preconditions;

		public HelpCommand(IImmutableCommand item)
			: base(item, item.Attributes, x => x is not IPrecondition)
		{
			IsAsyncFormattable = base.IsAsyncFormattable;

			ContextType = item.ContextType is Type c ? new HelpType(c) : null;
			var builder = ImmutableArray.CreateBuilder<HelpParameter>(item.Parameters.Count);
			foreach (var parameter in item.Parameters)
			{
				var helpParameter = new HelpParameter(parameter);
				if (helpParameter.IsAsyncFormattable)
				{
					IsAsyncFormattable = true;
				}
				builder.Add(helpParameter);
			}
			Parameters = builder.MoveToImmutable();
			Preconditions = item.Preconditions
				.Select(x => new HelpItem<IPrecondition>(x))
				.ToImmutableArray();
		}
	}
}