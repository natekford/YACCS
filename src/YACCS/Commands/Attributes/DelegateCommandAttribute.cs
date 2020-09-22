using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	public class DelegateCommandAttribute : Attribute
	{
		public Delegate Delegate { get; }

		public DelegateCommandAttribute(Delegate @delegate)
		{
			Delegate = @delegate;
		}
	}
}