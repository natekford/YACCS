using System;

namespace YACCS.Commands.Attributes
{
	public interface IDelegateCommandAttribute
	{
		Delegate Delegate { get; }
	}
}