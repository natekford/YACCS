using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace YACCS
{
	public static class ReflectionUtils
	{
		// Some interfaces Array implements
		// Don't deal with the non generic versions b/c how would we parse 'object'?
		public static readonly ImmutableArray<Type> SupportedEnumerableTypes = new[]
		{
			typeof(IList<>),
			typeof(ICollection<>),
			typeof(IEnumerable<>),
			typeof(IReadOnlyList<>),
			typeof(IReadOnlyCollection<>),
		}.ToImmutableArray();

		public static Lazy<T> CreateDelegate<T>(Func<T> createDelegateDelegate, string name)
		{
			return new Lazy<T>(() =>
			{
				try
				{
					return createDelegateDelegate();
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Unable to create {name}.", ex);
				}
			});
		}

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
					$"Unable to create an instance of {type.Name}. Is it missing a public parameterless constructor?", nameof(type), ex);
			}
			if (instance is T t)
			{
				return t;
			}
			throw new ArgumentException(
				$"{type.Name} does not implement {typeof(T).FullName}.", nameof(type));
		}

		public static Type? GetEnumerableType(this Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (type.IsInterface && type.IsGenericType)
			{
				var typeDefinition = type.GetGenericTypeDefinition();
				foreach (var supportedType in SupportedEnumerableTypes)
				{
					if (typeDefinition == supportedType)
					{
						return type.GetGenericArguments()[0];
					}
				}
			}
			return null;
		}

		public static (IEnumerable<PropertyInfo>, IEnumerable<FieldInfo>) GetWritableMembers(this Type type)
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

		public static bool IsGenericOf(this Type type, Type definition)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == definition;
	}
}