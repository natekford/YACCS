using System;

namespace YACCS
{
	public static class TypeRegistryUtils
	{
		public static T Get<T>(this ITypeRegistry<T> registry, Type type)
		{
			if (registry.TryGet(type, out var reader))
			{
				return reader;
			}
			throw new ArgumentException($"There is no {typeof(T).Name} registered for {type.Name}.", nameof(type));
		}
	}
}