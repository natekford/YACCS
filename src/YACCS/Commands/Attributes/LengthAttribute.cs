using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class LengthAttribute : Attribute, ILengthAttribute, IRuntimeFormattableAttribute
	{
		public int? Length { get; }
		protected virtual string LengthString { get; } = "Length";
		protected virtual string RemainderString { get; } = "Remainder";

		public LengthAttribute()
		{
			Length = null;
		}

		public LengthAttribute(int length)
		{
			Length = length;
		}

		public LengthAttribute(int? length)
		{
			Length = length;
		}

		public virtual IReadOnlyList<TaggedString> Format(IContext context)
		{
			return new TaggedString[]
			{
				new(Tag.Key, LengthString),
				new(Tag.Value, Length?.ToString() ?? RemainderString),
			};
		}
	}
}