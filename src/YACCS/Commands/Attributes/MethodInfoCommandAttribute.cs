using System;
using System.Reflection;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	public class MethodInfoCommandAttribute : Attribute
	{
		public MethodInfo Method { get; }

		public MethodInfoCommandAttribute(MethodInfo method)
		{
			Method = method;
		}
	}
}