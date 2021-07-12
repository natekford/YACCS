using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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
		public IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IParameterPrecondition>>> Preconditions { get; }
		public IHelpItem<ITypeReader>? TypeReader { get; }
		private string DebuggerDisplay => Item.FormatForDebuggerDisplay();

		public HelpParameter(IImmutableParameter item)
			: base(item, item.Attributes, x => x is not IParameterPrecondition)
		{
			ParameterType = Create(item.ParameterType);
			TypeReader = item.TypeReader is ITypeReader tr ? new HelpItem<ITypeReader>(tr) : null;

			Preconditions = item.Preconditions.ToImmutableDictionary(
				x => x.Key,
				x => x.Value
					.Select(x => (IHelpItem<IParameterPrecondition>)new HelpItem<IParameterPrecondition>(x))
					.ToLookup(x => x.Item.Op)
			);
		}
	}
}