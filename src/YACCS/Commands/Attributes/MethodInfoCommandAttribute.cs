using System;
using System.Reflection;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class MethodInfoCommandAttribute : Attribute
	{
		public MethodInfo Method { get; }

		public MethodInfoCommandAttribute(MethodInfo method)
		{
			Method = method;
		}
	}
}