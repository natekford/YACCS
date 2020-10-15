using System;
using System.Collections.Generic;

using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	public interface IHelpParameter : IHelpItem<IImmutableParameter>
	{
		bool IsRemainder { get; }
		IHelpItem<Type> ParameterType { get; }
		IReadOnlyList<IHelpItem<IParameterPrecondition>> Preconditions { get; }
		IHelpItem<ITypeReader>? TypeReader { get; }
	}
}