using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YACCS;

internal static class ReflectionUtils
{
	internal static T CreateDelegate<T>(
		Func<T> factory,
		[CallerArgumentExpression(nameof(factory))] string caller = "")
	{
		try
		{
			return factory();
		}
		catch (Exception ex)
		{
			// Throw a much more understandable exception
			throw new ArgumentException($"Unable to create the delegate '{caller}'.", ex);
		}
	}

	internal static T CreateInstance<T>(this Type type, params object[] args)
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