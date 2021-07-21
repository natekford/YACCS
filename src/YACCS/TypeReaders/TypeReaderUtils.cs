using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	public static class TypeReaderUtils
	{
		// HashSet<T> and its generic interfaces
		public static ImmutableHashSet<Type> HashSetTypes { get; } = new HashSet<Type>
		{
			typeof(ISet<>),
			typeof(HashSet<>),
		}.ToImmutableHashSet();

		// List<T> and its generic interfaces
		// Don't deal with the non generic versions b/c how would we parse 'object'?
		public static ImmutableHashSet<Type> ListTypes { get; } = new HashSet<Type>
		{
			typeof(IList<>),
			typeof(ICollection<>),
			typeof(IEnumerable<>),
			typeof(IReadOnlyList<>),
			typeof(IReadOnlyCollection<>),
			typeof(List<>),
		}.ToImmutableHashSet();

		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static Type? GetCollectionType(this Type type)
		{
			// If the type is an array, return its element type
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			return type.GetEnumerableType(ListTypes);
		}

		public static Type? GetHashSetType(this Type type)
			=> type.GetEnumerableType(HashSetTypes);

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

		private static Type? GetEnumerableType(this Type type, IImmutableSet<Type> defs)
		{
			if (type.IsGenericType && defs.Contains(type.GetGenericTypeDefinition()))
			{
				return type.GetGenericArguments()[0];
			}
			return null;
		}
	}
}