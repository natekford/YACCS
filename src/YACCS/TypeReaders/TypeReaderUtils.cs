using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	public static class TypeReaderUtils
	{
		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static ITypeReader<T> GetTypeReader<T>(
			this IReadOnlyDictionary<Type, ITypeReader> readers)
		{
			if (readers.TryGetValue(typeof(T), out var temp) && temp is ITypeReader<T> reader)
			{
				return reader;
			}
			throw new ArgumentException(
				$"Invalid type reader registered for {typeof(T).Name}.", nameof(T));
		}

		public static ITypeReader GetTypeReader(
			this IReadOnlyDictionary<Type, ITypeReader> readers,
			IImmutableParameter parameter)
			=> parameter.TypeReader ?? readers[parameter.ParameterType];

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				var attribute = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
				if (attribute is null)
				{
					continue;
				}

				var typeReader = type.CreateInstance<ITypeReader>();
				yield return new(attribute.TargetTypes, typeReader);
			}
		}

		public static void RegisterTypeReaders(
			this IDictionary<Type, ITypeReader> readers,
			IEnumerable<TypeReaderInfo> typeReaders)
		{
			foreach (var typeReader in typeReaders)
			{
				foreach (var type in typeReader.TargetTypes)
				{
					readers[type] = typeReader.Instance;
				}
			}
		}

		public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
		{
			if (!type.IsAssignableFrom(reader.OutputType))
			{
				throw new ArgumentException(
					$"A type reader with the output type {reader.OutputType.Name} " +
					$"cannot be used for the type {type.Name}.", nameof(reader));
			}
		}
	}
}