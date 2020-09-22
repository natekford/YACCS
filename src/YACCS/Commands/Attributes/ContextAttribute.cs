using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ContextAttribute : LengthAttribute
	{
		public ContextAttribute() : base(0)
		{
		}
	}
}