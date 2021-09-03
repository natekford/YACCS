using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace YACCS
{
	public static class ReflectionUtils
	{
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
}