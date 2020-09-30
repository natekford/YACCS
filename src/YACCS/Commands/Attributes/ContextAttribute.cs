using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ContextAttribute : CountAttribute
	{
		public ContextAttribute() : base(0)
		{
		}
	}
}