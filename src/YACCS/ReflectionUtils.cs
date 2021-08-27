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

		internal static TryExpression AddThrow<T>(
			this Expression body,
			Expression<Action<T>> createException)
			where T : Exception
		{
			var createExceptionBody = (NewExpression)createException.Body;
			var caughtException = createExceptionBody.Arguments
				.OfType<ParameterExpression>()
				.Single(x => x.Type == typeof(T));
			var @throw = Expression.Throw(createExceptionBody);
			var @catch = Expression.Catch(caughtException, @throw);
			return Expression.TryCatch(body, @catch);
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

		internal static IEnumerable<T> CreateExpressionsForWritableMembers<T>(
			this Type type,
			Expression instance,
			Func<MemberExpression, T> createExpression)
			where T : Expression
		{
			T Convert(MemberInfo x)
			{
				var instanceCast = Expression.Convert(instance, x.DeclaringType);
				var access = Expression.MakeMemberAccess(instanceCast, x);
				return createExpression(access);
			}

			var (properties, fields) = type.GetWritableMembers();
			var propertyExpressions = properties.Select(Convert);
			var fieldExpressions = fields.Select(Convert);
			return propertyExpressions.Concat(fieldExpressions);
		}

		internal static (Expression Body, ParameterExpression Args) CreateInvokeExpressionFromObjectArrayArgs(
			this Expression? instance,
			MethodInfo method)
		{
			// Make sure we're calling the method on something that actually implements it
			if (instance is not null)
			{
				instance = Expression.Convert(instance, method.DeclaringType);
			}

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
				body = Expression.Block(body, @null);
			}
			// Value types need to be boxed
			// This has to go after the void check because void is a value type
			else if (method.ReturnType.IsValueType)
			{
				body = Expression.Convert(body, typeof(object));
			}

			return (body, args);
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