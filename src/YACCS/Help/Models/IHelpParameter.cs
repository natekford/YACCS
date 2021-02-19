using System;
using System.Collections.Generic;

using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.TypeReaders;

namespace YACCS.Help.Models
{
	public interface IHelpParameter : IHelpItem<IImmutableParameter>, IHasPreconditions
	{
		bool IsRemainder { get; }
		IHelpItem<Type> ParameterType { get; }
		new IReadOnlyList<IHelpItem<IParameterPrecondition>> Preconditions { get; }
		IHelpItem<ITypeReader>? TypeReader { get; }
	}
}