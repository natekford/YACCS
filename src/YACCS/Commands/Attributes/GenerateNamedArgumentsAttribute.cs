using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateNamedArgumentsAttribute : Attribute
	{
	}
}