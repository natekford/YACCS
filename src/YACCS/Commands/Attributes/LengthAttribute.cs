using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class LengthAttribute : Attribute, ILengthAttribute
	{
		public int Length { get; }

		public LengthAttribute(int length)
		{
			Length = length;
		}
	}
}