using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = false)]
	public class HiddenAttribute : Attribute, IHiddenAttribute
	{
	}
}