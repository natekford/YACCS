using System;

namespace YACCS.SwappedArguments
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class SwappableAttribute : Attribute, ISwappableAttribute
	{
	}
}