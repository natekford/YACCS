using System;

namespace YACCS.SwapArguments
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class SwappableAttribute : Attribute, ISwappableAttribute
	{
	}
}