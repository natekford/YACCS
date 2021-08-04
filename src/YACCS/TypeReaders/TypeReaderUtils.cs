using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

		public static void ThrowIfUnregisteredServices(
			this IReadOnlyDictionary<Type, ITypeReader> readers,
			IServiceProvider services)
		{
			foreach (var (_, reader) in readers)
			{
				reader.ThrowIfUnregisteredServices(services);
			}
		}

		public static void ThrowIfUnregisteredServices(
			this ITypeReader reader,
			IServiceProvider services)
		{
			const BindingFlags FLAGS = 0
				| BindingFlags.Static
				| BindingFlags.NonPublic;

			var args = new[] { services };
			foreach (var method in reader.GetType().GetMethods(FLAGS))
			{
				if (method.GetCustomAttribute<GetServiceMethodAttribute>() is null)
				{
					continue;
				}

				try
				{
					_ = method.Invoke(null, args);
				}
				catch (Exception e)
				{
					throw new InvalidOperationException("Unable to get service for " +
						$"{method} declared in {method.DeclaringType}.", e);
				}
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

		// These are only here to prevent adding a service provider implementation dependency.
		internal static T GetRequiredService<T>(this IServiceProvider provider)
		{
			var service = provider.GetService(typeof(T));
			if (service is T t)
			{
				return t;
			}
			throw new InvalidOperationException(
				$"{typeof(T).Name} does not have a registered service.");
		}

		internal static T GetService<T>(this IServiceProvider provider, T @default)
			=> provider.GetService(typeof(T)) is T t ? t : @default;

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