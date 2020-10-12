using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class RemainderAttribute : CountAttribute
	{
		public RemainderAttribute() : base(null)
		{
		}
	}
}