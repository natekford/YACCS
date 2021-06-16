using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.Preconditions;

namespace YACCS.Help.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class HelpCommand : HelpItem<IImmutableCommand>, IHelpCommand
	{
		public IHelpItem<Type>? ContextType { get; }
		public bool HasAsyncFormattableParameters { get; }
		public bool HasAsyncFormattablePreconditions { get; }
		public IReadOnlyList<IHelpParameter> Parameters { get; }
		public IReadOnlyList<IHelpItem<IPrecondition>> Preconditions { get; }
		IReadOnlyList<IHelpItem<object>> IHasPreconditions.Preconditions => Preconditions;
		private string DebuggerDisplay => $"Name = {Item.Names[0]}, Parameter Count = {Item.Parameters.Count}";

		public HelpCommand(IImmutableCommand item)
			: base(item, item.Attributes, x => x is not IPrecondition)
		{
			ContextType = item.ContextType is Type c ? HelpItemUtils.Create(c) : null;

			{
				var builder = ImmutableArray.CreateBuilder<HelpParameter>(item.Parameters.Count);
				foreach (var parameter in item.Parameters)
				{
					var help = new HelpParameter(parameter);
					if (help.IsAsyncFormattable())
					{
						HasAsyncFormattableParameters = true;
					}
					builder.Add(help);
				}
				Parameters = builder.MoveToImmutable();
			}

			{
				var builder = ImmutableArray.CreateBuilder<HelpItem<IPrecondition>>(item.Preconditions.Count);
				foreach (var group in item.Preconditions)
				{
					foreach (var precondition in group.Value)
					{
						var help = new HelpItem<IPrecondition>(precondition);
						if (help.IsAsyncFormattable())
						{
							HasAsyncFormattablePreconditions = true;
						}
						builder.Add(help);
					}
				}
				Preconditions = builder.MoveToImmutable();
			}
		}
	}
}