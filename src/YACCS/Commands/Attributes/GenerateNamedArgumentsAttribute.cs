using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GenerateNamedArgumentsAttribute : Attribute
	{
	}
}