using System;
using System.Collections.Generic;

using YACCS.Help;

namespace YACCS.Examples
{
	public class ConsoleTagFormatter : TagFormatter
	{
		private readonly IReadOnlyDictionary<Type, string> _Names;

		public ConsoleTagFormatter(IReadOnlyDictionary<Type, string> names)
		{
			_Names = names;
		}

		public override string Format(
			string? format,
			object? arg,
			IFormatProvider formatProvider)
		{
			if (arg is Type type)
			{
				arg = _Names[type];
			}
			return base.Format(format, arg, formatProvider);
		}
	}
}