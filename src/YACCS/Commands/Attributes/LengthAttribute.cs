using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class LengthAttribute : Attribute, ILengthAttribute, IRuntimeFormattableAttribute
	{
		public int? Length { get; }

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
				new(Tag.Key, Localize.This("Length")),
				new(Tag.Value, Length?.ToString() ?? Localize.This("Remainder")),
			};
		}
	}
}