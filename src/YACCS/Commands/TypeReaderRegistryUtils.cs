using System;
using System.Collections.Generic;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.TypeReaders;

namespace YACCS.Commands
{
	public static class TypeReaderRegistryUtils
	{
		public static ITypeReader GetReader(this ITypeReaderRegistry registry, Type type)
		{
			if (registry.TryGetReader(type, out var reader))
			{
				return reader;
			}
			throw new ArgumentException($"There is no converter specified for {type.Name}.", nameof(type));
		}

		public static ITypeReader<T> GetReader<T>(this ITypeReaderRegistry registry)
		{
			if (registry.GetReader(typeof(T)) is ITypeReader<T> reader)
			{
				return reader;
			}
			throw new ArgumentException($"Invalid converter registered for {typeof(T).Name}.", nameof(T));
		}

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(this IEnumerable<Assembly> assemblies)
		{
			foreach (var assembly in assemblies)
			{
				foreach (var typeReader in assembly.GetTypeReaders())
				{
					yield return typeReader;
				}
			}
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

				var typeReader = CommandServiceUtils.CreateInstance<ITypeReader>(type);
				yield return new TypeReaderInfo(attr.TargetTypes, typeReader);
			}
		}

		public static void Register<T>(this ITypeReaderRegistry registry, ITypeReader<T> reader)
			=> registry.Register(reader, typeof(T));

		public static void Register(
			this ITypeReaderRegistry registry,
			IEnumerable<TypeReaderInfo> typeReaderInfos)
		{
			foreach (var typeReaderInfo in typeReaderInfos)
			{
				foreach (var type in typeReaderInfo.TargetTypes)
				{
					registry.Register(typeReaderInfo.Instance, type);
				}
			}
		}
	}
}