using System;

using YACCS.Commands.Models;

namespace YACCS.Commands
{

	public static class ParameterUtils
	{
		// Where there is no default value, ParameterInfo.DefaultValue is a DBNull instance
		public static bool IsOptional(this IImmutableParameter parameter)
			=> parameter.DefaultValue != DBNull.Value;

		public static bool IsOptional(this IParameter parameter)
			=> parameter.DefaultValue != DBNull.Value;
	}
}