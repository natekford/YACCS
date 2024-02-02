using MorseCode.ITask;

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using YACCS.Commands.Models;

namespace YACCS.TypeReaders;

/// <summary>
/// Utilites for type readers.
/// </summary>
public static class TypeReaderUtils
{
	// Don't deal with the non generic versions b/c how would we parse 'object'?
	// HashSet<T> and its generic interfaces
	private static FrozenSet<Type> HashSetTypes { get; } = new HashSet<Type>
	{
		typeof(HashSet<>),
		typeof(ISet<>),
	}.ToFrozenSet();

	// List<T> and its generic interfaces
	private static FrozenSet<Type> ListTypes { get; } = new HashSet<Type>
	{
		typeof(List<>),
		typeof(IList<>),
		typeof(ICollection<>),
		typeof(IEnumerable<>),
		typeof(IReadOnlyList<>),
		typeof(IReadOnlyCollection<>),
	}.ToFrozenSet();

	/// <summary>
	/// Converts <paramref name="result"/> to <see cref="ITask{TResult}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="result">The result to convert.</param>
	/// <returns>A task wrapping <paramref name="result"/>.</returns>
	public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
		=> Task.FromResult(result).AsITask();

	/// <summary>
	/// Gets all type readers defined in <paramref name="assembly"/>.
	/// </summary>
	/// <remarks>
	/// Any class marked with <see cref="TypeReaderTargetTypesAttribute"/> will be
	/// retrieved and instantiated. Each class must implement <see cref="ITypeReader"/>.
	/// </remarks>
	/// <param name="assembly">The assembly to search through.</param>
	/// <returns>An enumerable of type readers.</returns>
	public static IEnumerable<TypeReaderInfo> GetCustomTypeReaders(this Assembly assembly)
	{
		foreach (var type in assembly.GetExportedTypes())
		{
			var attribute = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
			if (attribute is null)
			{
				continue;
			}

			yield return new(
				attribute.TargetTypes,
				attribute.OverrideExistingTypeReaders,
				type.CreateInstance<ITypeReader>()
			);
		}
	}

	/// <summary>
	/// Gets the type reader registered for <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="readers">The type readers to search in.</param>
	/// <returns>The type reader for <typeparamref name="T"/>.</returns>
	/// <exception cref="ArgumentException">
	/// When no type reader is registered for <typeparamref name="T"/>
	/// - or -
	/// When the retrieved type reader is not the correct generic type.
	/// </exception>
	public static ITypeReader<T> GetTypeReader<T>(
		this IReadOnlyDictionary<Type, ITypeReader> readers)
	{
		if (!readers.TryGetValue(typeof(T), out var temp))
		{
			throw new ArgumentException(
				$"No type reader registered for {typeof(T).Name}.", nameof(T));
		}
		if (temp is not ITypeReader<T> reader)
		{
			throw new ArgumentException(
				$"Invalid type reader registered for {typeof(T).Name}.", nameof(T));
		}
		return reader;
	}

	/// <summary>
	/// Gets the type reader to use for parsing <paramref name="parameter"/>.
	/// </summary>
	/// <param name="readers">The type readers to search in.</param>
	/// <param name="parameter">The parameter to get a type reader for.</param>
	/// <returns>A type reader for <paramref name="parameter"/>.</returns>
	public static ITypeReader GetTypeReader(
		this IReadOnlyDictionary<Type, ITypeReader> readers,
		IImmutableParameter parameter)
		=> parameter.TypeReader ?? readers[parameter.ParameterType];

	/// <summary>
	/// Throws an exception if the output type of <paramref name="reader"/> cannot
	/// be assigned to a variable of <paramref name="type"/>.
	/// </summary>
	/// <param name="reader">The reader to validate.</param>
	/// <param name="type">The type to validate for.</param>
	/// <exception cref="ArgumentException">
	/// When the output type of <paramref name="reader"/> cannot be assigned to
	/// a variable of <paramref name="type"/>.
	/// </exception>
	public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
	{
		if (!type.IsAssignableFrom(reader.OutputType))
		{
			throw new ArgumentException(
				$"A type reader with the output type {reader.OutputType.Name} " +
				$"cannot be used for the type {type.Name}.", nameof(reader));
		}
	}

	/// <summary>
	/// Throws an exception if any <paramref name="readers"/> requires a service which
	/// <paramref name="services"/> does not currently have created.
	/// </summary>
	/// <remarks>
	/// Each <paramref name="readers"/> must have <see langword="static"/> methods marked
	/// with <see cref="GetServiceMethodAttribute"/> for the method to be validated.
	/// </remarks>
	/// <param name="readers">The type readers to validate.</param>
	/// <param name="services">The services which are currently registered.</param>
	/// <exception cref="InvalidOperationException">
	/// When any <paramref name="readers"/> requires a service which
	/// <paramref name="services"/> does not currently have created.
	/// </exception>
	public static void ThrowIfUnregisteredServices(
		this IReadOnlyDictionary<Type, ITypeReader> readers,
		IServiceProvider services)
	{
		foreach (var reader in readers.Values)
		{
			reader.ThrowIfUnregisteredServices(services);
		}
	}

	/// <summary>
	/// Throws an exception if <paramref name="reader"/> requires a service which
	/// <paramref name="services"/> does not currently have created.
	/// </summary>
	/// <remarks>
	/// <paramref name="reader"/> must have <see langword="static"/> methods marked
	/// with <see cref="GetServiceMethodAttribute"/> for the method to be validated.
	/// </remarks>
	/// <param name="reader">The type reader to validate.</param>
	/// <param name="services">The services which are currently registered.</param>
	/// <exception cref="InvalidOperationException">
	/// When <paramref name="reader"/> requires a service which
	/// <paramref name="services"/> does not currently have created.
	/// </exception>
	public static void ThrowIfUnregisteredServices(
		this ITypeReader reader,
		IServiceProvider services)
	{
		const BindingFlags FLAGS = 0
			| BindingFlags.Static
			| BindingFlags.Public
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
				throw new InvalidOperationException("Unable to get a service for " +
					$"{method} declared in {method.DeclaringType}.", e);
			}
		}
	}

	/// <summary>
	/// Gets the generic type argument used for a collection.
	/// </summary>
	/// <param name="type">The type to look at.</param>
	/// <param name="collectionType">The retrieved collection type.</param>
	/// <returns>A bool indicating success or failure.</returns>
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

	/// <summary>
	/// Gets the generic type argument used for a set.
	/// </summary>
	/// <param name="type">The type to look at.</param>
	/// <param name="setType">The retrieved set type.</param>
	/// <returns>A bool indicating success or failure.</returns>
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
		ISet<Type> defs,
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