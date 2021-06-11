using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace YACCS
{
	public static class ReflectionUtils
	{
		// Some interfaces Array implements
		// Don't deal with the non generic versions b/c how would we parse 'object'?
		public static readonly HashSet<Type> SupportedArrayInterfaces = new()
		{
			typeof(IList<>),
			typeof(ICollection<>),
			typeof(IEnumerable<>),
			typeof(IReadOnlyList<>),
			typeof(IReadOnlyCollection<>),
		};

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
					$"Unable to create an instance of {type.Name}.", nameof(type), ex);
			}
			if (instance is T t)
			{
				return t;
			}
			throw new ArgumentException(
				$"{type.Name} does not implement {typeof(T).FullName}.", nameof(type));
		}

		public static (Expression Body, ParameterExpression Args) CreateInvokeDelegate(this Expression? instance, MethodInfo method)
		{
			var args = Expression.Parameter(typeof(object?[]), "Args");
			var argsCast = method.GetParameters().Select((x, i) =>
			{
				var access = Expression.ArrayAccess(args, Expression.Constant(i));
				return Expression.Convert(access, x.ParameterType);
			});
			Expression body = Expression.Call(instance, method, argsCast);

			// With a return type of void to keep the Func<object?[], object> declaration
			// we just need to return a null value at the end
			if (method.ReturnType == typeof(void))
			{
				var @null = Expression.Constant(null);
				var objectLabel = Expression.Label(typeof(object));
				var @return = Expression.Return(objectLabel, @null, typeof(object));
				var returnLabel = Expression.Label(objectLabel, @null);
				body = Expression.Block(body, @return, returnLabel);
			}
			// Value types need to be boxed
			// This has to go after the void check because void is a value type
			else if (method.ReturnType.IsValueType)
			{
				body = Expression.Convert(body, typeof(object));
			}

			return (body, args);
		}

		public static Type? GetArrayType(this Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (type.IsInterface && type.IsGenericType
				&& SupportedArrayInterfaces.Contains(type.GetGenericTypeDefinition()))
			{
				return type.GetGenericArguments()[0];
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