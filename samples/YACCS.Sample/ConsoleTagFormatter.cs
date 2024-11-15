﻿using YACCS.Help;

namespace YACCS.Sample;

public sealed class ConsoleTagFormatter(IReadOnlyDictionary<Type, string> names)
	: TagFormatter
{
	private readonly IReadOnlyDictionary<Type, string> _Names = names;

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