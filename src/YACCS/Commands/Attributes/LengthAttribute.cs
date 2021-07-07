﻿using System;

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

		public virtual string Format(IContext context, IFormatProvider? formatProvider = null)
			=> formatProvider.Format($"{Keys.LENGTH:k} {Length?.ToString() ?? Keys.REMAINDER:v}");
	}
}