using System.Reflection;

namespace YACCS;

/// <summary>
/// Utilities for reflection.
/// </summary>
public static class ReflectionUtils
{
	/// <summary>
	/// Creates a new <paramref name="type"/> and then casts it to <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="type">The type to create.</param>
	/// <param name="args">The arguments for the constructor.</param>
	/// <returns>A new instance of <typeparamref name="T"/>.</returns>
	public static T CreateInstance<T>(this Type type, params object[] args)
	{
		object instance;
		try
		{
			instance = Activator.CreateInstance(type, args);
		}
		catch (Exception ex)
		{
			throw new ArgumentException(
				$"Unable to create an instance of {type.Name}.", nameof(type), ex);
		}
		if (instance is T t)
		{
			return t;
		}
		throw new ArgumentException(
			$"{type.Name} does not implement {typeof(T).FullName}.", nameof(type));
	}

	internal static T CreateDelegate<T>(Func<T> factory, string name)
	{
		try
		{
			return factory();
		}
		catch (Exception ex)
		{
			throw new ArgumentException($"Unable to create the delegate '{name}'.", ex);
		}
	}

	internal static (IEnumerable<PropertyInfo>, IEnumerable<FieldInfo>) GetWritableMembers(
		this Type type)
	{
		const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance;
		var properties = type
			.GetProperties(FLAGS)
			.Where(x => x.CanWrite && x.SetMethod?.IsPublic == true);
		var fields = type
			.GetFields(FLAGS)
			.Where(x => !x.IsInitOnly);
		return (properties, fields);
	}

	internal static bool IsGenericOf(this Type type, Type definition)
		=> type.IsGenericType && type.GetGenericTypeDefinition() == definition;
}