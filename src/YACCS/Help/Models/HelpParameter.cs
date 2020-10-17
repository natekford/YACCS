using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class HelpParameter : HelpItem<IImmutableParameter>, IHelpParameter
	{
		public bool HasAsyncFormattablePreconditions { get; }
		public bool IsRemainder => Item.Length == null;
		public IHelpItem<Type> ParameterType { get; }
		public IReadOnlyList<IHelpItem<IParameterPrecondition>> Preconditions { get; }
		public IHelpItem<ITypeReader>? TypeReader { get; }
		IReadOnlyList<IHelpItem<object>> IHasPreconditions.Preconditions => Preconditions;
		private string DebuggerDisplay => $"Name = {Item.ParameterName}, Type = {Item.ParameterType}";

		public HelpParameter(IImmutableParameter item)
			: base(item, item.Attributes, x => x is not IParameterPrecondition)
		{
			ParameterType = HelpItemUtils.Create(item.ParameterType);
			TypeReader = item.TypeReader is ITypeReader tr ? new HelpItem<ITypeReader>(tr) : null;

			{
				var array = ImmutableArray.CreateBuilder<HelpItem<IParameterPrecondition>>(item.Preconditions.Count);
				foreach (var precondition in item.Preconditions)
				{
					var help = new HelpItem<IParameterPrecondition>(precondition);
					if (help.HasAsyncFormattableAttributes)
					{
						HasAsyncFormattablePreconditions = true;
					}
					array.Add(help);
				}
				Preconditions = array.MoveToImmutable();
			}
		}
	}
}