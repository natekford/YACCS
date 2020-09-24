using System;
using System.Collections.Generic;

namespace YACCS.Commands.Attributes
{
	public interface ITypeReaderTargetTypeAttribute
	{
		IReadOnlyList<Type> TargetTypes { get; }
	}
}