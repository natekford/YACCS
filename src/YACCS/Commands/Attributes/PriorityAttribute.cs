using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class PriorityAttribute : Attribute, IPriorityAttribute
	{
		public int Priority { get; }

		public PriorityAttribute(int priority)
		{
			Priority = priority;
		}
	}
}