using System;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class CountAttribute : Attribute, ICountAttribute
	{
		public int? Count { get; }

		public CountAttribute()
		{
			Count = null;
		}

		public CountAttribute(int length)
		{
			Count = length;
		}

		public CountAttribute(int? length)
		{
			Count = length;
		}
	}
}