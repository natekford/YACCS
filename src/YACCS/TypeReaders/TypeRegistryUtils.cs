using System;
using System.Collections.Generic;
using System.Reflection;

using YACCS.Commands;

namespace YACCS.TypeReaders
{
	public static class TypeRegistryUtils
	{
		public static ITypeReader<T> Get<T>(this ITypeRegistry<ITypeReader> registry)
		{
			if (registry.Get(typeof(T)) is ITypeReader<T> reader)
			{
				return reader;
			}
			throw new ArgumentException($"Invalid type reader registered for {typeof(T).Name}.", nameof(T));
		}

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				var attr = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
				if (attr == null)
				{
					continue;
				}

				var typeReader = ReflectionUtils.CreateInstance<ITypeReader>(type);
				yield return new TypeReaderInfo(attr.TargetTypes, typeReader);
			}
		}

		public static void Register(
			this ITypeRegistry<ITypeReader> registry,
			IEnumerable<TypeReaderInfo> typeReaderInfos)
		{
			foreach (var typeReaderInfo in typeReaderInfos)
			{
				foreach (var type in typeReaderInfo.TargetTypes)
				{
					registry.Register(type, typeReaderInfo.Instance);
				}
			}
		}

		public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
		{
			if (!type.IsAssignableFrom(reader.OutputType))
			{
				throw new ArgumentException(
					$"A type reader with the output type {reader.OutputType.Name} " +
					$"cannot be used for a the type {type.Name}.", nameof(reader));
			}
		}
	}
}