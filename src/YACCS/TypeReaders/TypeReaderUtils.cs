using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	public static class TypeReaderUtils
	{
		// Don't deal with the non generic versions b/c how would we parse 'object'?

		// HashSet<T> and its generic interfaces
		public static ImmutableHashSet<Type> HashSetTypes { get; } = new HashSet<Type>
		{
			typeof(HashSet<>),
			typeof(ISet<>),
		}.ToImmutableHashSet();

		// List<T> and its generic interfaces
		public static ImmutableHashSet<Type> ListTypes { get; } = new HashSet<Type>
		{
			typeof(List<>),
			typeof(IList<>),
			typeof(ICollection<>),
			typeof(IEnumerable<>),
			typeof(IReadOnlyList<>),
			typeof(IReadOnlyCollection<>),
		}.ToImmutableHashSet();

		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static IEnumerable<TypeReaderInfo> GetCustomTypeReaders(this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				var attribute = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
				if (attribute is null)
				{
					continue;
				}

				var typeReader = type.CreateInstance<ITypeReader>();
				yield return new(attribute, typeReader);
			}
		}

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

		public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
		{
			if (!type.IsAssignableFrom(reader.OutputType))
			{
				throw new ArgumentException(
					$"A type reader with the output type {reader.OutputType.Name} " +
					$"cannot be used for the type {type.Name}.", nameof(reader));
			}
		}

		public static bool TryGetCollectionType(
			this Type type,
			[NotNullWhen(true)] out Type? collectionType)
		{
			// If the type is an array, return its element type
			if (type.IsArray)
			{
				collectionType = type.GetElementType();
				return true;
			}
			return type.TryGetEnumerableType(ListTypes, out collectionType);
		}

		public static bool TryGetHashSetType(
			this Type type,
			[NotNullWhen(true)] out Type? setType)
			=> type.TryGetEnumerableType(HashSetTypes, out setType);

		private static bool TryGetEnumerableType(
			this Type type,
			IImmutableSet<Type> defs,
			[NotNullWhen(true)] out Type? enumerableType)
		{
			if (type.IsGenericType && defs.Contains(type.GetGenericTypeDefinition()))
			{
				enumerableType = type.GetGenericArguments()[0];
				return true;
			}
			enumerableType = null;
			return false;
		}
	}
}