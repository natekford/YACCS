using System;
using System.Reflection;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class MethodInfoCommandAttribute : Attribute, IMethodInfoCommandAttribute
	{
		public MethodInfo Method { get; }

		public MethodInfoCommandAttribute(MethodInfo method)
		{
			Method = method;
		}
	}
}