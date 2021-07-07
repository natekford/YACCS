using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class HelpCommand : HelpItem<IImmutableCommand>, IHelpCommand
	{
		public IHelpItem<Type>? ContextType { get; }
		public IReadOnlyList<IHelpParameter> Parameters { get; }
		public IReadOnlyList<IHelpItem<IPrecondition>> Preconditions { get; }
		IReadOnlyList<IHelpItem<object>> IHasPreconditions.Preconditions => Preconditions;
		private string DebuggerDisplay => Item.FormatForDebuggerDisplay();

		public HelpCommand(IImmutableCommand item)
			: base(item, item.Attributes, x => x is not IPrecondition)
		{
			ContextType = item.ContextType is Type c ? HelpItemUtils.Create(c) : null;

			{
				var builder = ImmutableArray.CreateBuilder<HelpParameter>(item.Parameters.Count);
				foreach (var parameter in item.Parameters)
				{
					builder.Add(new HelpParameter(parameter));
				}
				Parameters = builder.MoveToImmutable();
			}

			{
				var builder = ImmutableArray.CreateBuilder<HelpItem<IPrecondition>>(item.Preconditions.Count);
				foreach (var group in item.Preconditions)
				{
					foreach (var precondition in group.Value)
					{
						builder.Add(new HelpItem<IPrecondition>(precondition));
					}
				}
				Preconditions = builder.MoveToImmutable();
			}
		}
	}
}