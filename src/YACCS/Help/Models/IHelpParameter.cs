using System;
using System.Collections.Generic;
using System.Linq;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	public interface IHelpParameter : IHelpItem<IImmutableParameter>
	{
		bool IsRemainder { get; }
		IHelpItem<Type> ParameterType { get; }
		IReadOnlyDictionary<string, ILookup<BoolOp, IHelpItem<IParameterPrecondition>>> Preconditions { get; }
		IHelpItem<ITypeReader>? TypeReader { get; }
	}
}