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

		internal static TValue ThrowIfDuplicate<TAttribute, TValue>(
			this TAttribute attribute,
			Func<TAttribute, TValue> converter,
			ref int count)
		{
			if (count > 0)
			{
				var name = typeof(TAttribute).Name;
				throw new InvalidOperationException($"Duplicate {name} attribute.");
			}

			++count;
			return converter.Invoke(attribute);
		}
	}
}