using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class ContextAttribute : CountAttribute
	{
		public ContextAttribute() : base(0)
		{
		}
	}
}