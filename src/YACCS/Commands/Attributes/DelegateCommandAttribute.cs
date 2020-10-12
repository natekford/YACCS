using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class DelegateCommandAttribute : Attribute, IDelegateCommandAttribute
	{
		public Delegate Delegate { get; }

		public DelegateCommandAttribute(Delegate @delegate)
		{
			Delegate = @delegate;
		}
	}
}