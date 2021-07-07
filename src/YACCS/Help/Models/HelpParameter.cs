using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class HelpParameter : HelpItem<IImmutableParameter>, IHelpParameter
	{
		public bool IsRemainder => Item.Length == null;
		public IHelpItem<Type> ParameterType { get; }
		public IReadOnlyList<IHelpItem<IParameterPrecondition>> Preconditions { get; }
		public IHelpItem<ITypeReader>? TypeReader { get; }
		IReadOnlyList<IHelpItem<object>> IHasPreconditions.Preconditions => Preconditions;
		private string DebuggerDisplay => Item.FormatForDebuggerDisplay();

		public HelpParameter(IImmutableParameter item)
			: base(item, item.Attributes, x => x is not IParameterPrecondition)
		{
			ParameterType = HelpItemUtils.Create(item.ParameterType);
			TypeReader = item.TypeReader is ITypeReader tr ? new HelpItem<ITypeReader>(tr) : null;

			{
				var builder = ImmutableArray.CreateBuilder<HelpItem<IParameterPrecondition>>(item.Preconditions.Count);
				foreach (var precondition in item.Preconditions)
				{
					builder.Add(new HelpItem<IParameterPrecondition>(precondition));
				}
				Preconditions = builder.MoveToImmutable();
			}
		}
	}
}