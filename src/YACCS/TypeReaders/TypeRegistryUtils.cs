using System;
using System.Collections.Generic;
using System.Reflection;

using YACCS.Commands.Models;

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

		public static (bool MakeArray, ITypeReader Reader) Get(
			this ITypeRegistry<ITypeReader> registry,
			IImmutableParameter parameter)
		{
			// TypeReader is overridden, we /shouldn't/ need to deal with converting
			// to an enumerable for the dev
			if (parameter.TypeReader is not null)
			{
				return (false, parameter.TypeReader);
			}
			// Parameter type is directly in the TypeReader collection, use that
			var pType = parameter.ParameterType;
			if (registry.TryGet(pType, out var reader))
			{
				return (false, reader);
			}
			// Parameter type is not, but the parameter is an enumerable and its enumerable
			// type is in the TypeReader collection.
			// Let's read each value for the enumerable separately
			var eType = parameter.ElementType;
			if (eType is not null && registry.TryGet(eType, out reader))
			{
				return (true, reader);
			}
			throw new ArgumentException($"There is no converter specified for {parameter.ParameterType.Name}.");
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

				var typeReader = type.CreateInstance<ITypeReader>();
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