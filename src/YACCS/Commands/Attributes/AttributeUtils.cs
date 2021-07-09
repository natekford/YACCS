using System;
using System.Collections.Immutable;

using YACCS.Commands.Models;

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

		public static ImmutableArray<object> CreateGeneratedCommandAttributeList(
			this IImmutableCommand source)
		{
			var builder = ImmutableArray.CreateBuilder<object>(source.Attributes.Count + 1);
			builder.AddRange(source.Attributes);
			builder.Add(new GeneratedCommandAttribute(source));
			return builder.MoveToImmutable();
		}

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