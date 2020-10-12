using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class CountAttribute : Attribute, ILengthAttribute
	{
		public int? Length { get; }

		public CountAttribute()
		{
			Length = null;
		}

		public CountAttribute(int length)
		{
			Length = length;
		}

		public CountAttribute(int? length)
		{
			Length = length;
		}
	}
}