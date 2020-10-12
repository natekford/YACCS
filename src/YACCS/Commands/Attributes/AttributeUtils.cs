using System;

namespace YACCS.Commands.Attributes
{
	public static class AttributeUtils
	{
		public const AttributeTargets COMMANDS = 0
			| AttributeTargets.Method;

		public const AttributeTargets PARAMETERS = 0
			| AttributeTargets.Parameter
			| AttributeTargets.Property
			| AttributeTargets.Field;
	}
}