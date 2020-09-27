using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class RemainderAttribute : LengthAttribute
	{
		public RemainderAttribute() : base(null)
		{
		}
	}
}