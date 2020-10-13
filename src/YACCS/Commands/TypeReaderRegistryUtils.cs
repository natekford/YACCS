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
			throw new ArgumentException($"There is no type reader registered for {type.Name}.", nameof(type));
		}

		public static ITypeReader<T> GetReader<T>(this ITypeReaderRegistry registry)
		{
			if (registry.GetReader(typeof(T)) is ITypeReader<T> reader)
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