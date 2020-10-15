using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	public class HelpParameter : HelpItem<IImmutableParameter>, IHelpParameter
	{
		public bool IsRemainder => Item.Length == null;
		public IHelpItem<Type> ParameterType { get; }
		public IReadOnlyList<IHelpItem<IParameterPrecondition>> Preconditions { get; }
		public IHelpItem<ITypeReader>? TypeReader { get; }

		public HelpParameter(IImmutableParameter item)
			: base(item, item.Attributes, x => x is not IParameterPrecondition)
		{
			ParameterType = new HelpType(item.ParameterType);
			TypeReader = item.TypeReader is ITypeReader tr ? new HelpItem<ITypeReader>(tr) : null;
			Preconditions = item.Preconditions
				.Select(x => new HelpItem<IParameterPrecondition>(x))
				.ToImmutableArray();
		}
	}
}