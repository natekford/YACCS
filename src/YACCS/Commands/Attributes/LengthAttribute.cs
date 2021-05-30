using System;
using System.Collections.Generic;

using YACCS.Help;
using YACCS.Help.Attributes;

namespace YACCS.Commands.Attributes
{
	[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
	public class LengthAttribute : Attribute, ILengthAttribute, IRuntimeFormattableAttribute
	{
		private static readonly TaggedString _Key = new(Tag.Key, "Length");

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
			return new[]
			{
				_Key,
				new TaggedString(Tag.Value, Length?.ToString() ?? "Remainder"),
			};
		}
	}
}