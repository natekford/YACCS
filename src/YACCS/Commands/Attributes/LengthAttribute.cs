using System;
using System.Threading.Tasks;

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

		public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		{
			var value = Length ?? (object?)Keys.Remainder;
			return new(formatProvider.Format($"{Keys.Length:key} {value:value}"));
		}
	}
}