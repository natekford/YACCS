using System.Linq.Expressions;
using System.Reflection;

namespace YACCS
{
	internal static class ExpressionUtils
	{
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

		internal static T GetInvokeFromObjectArray<T>(
			Expression? instance,
			MethodInfo method)
		{
			/*
			 *	(object?[] Args) =>
			 *	{
			 *		return ((DelegateType)Delegate).Invoke((ParamType)Args[0], (ParamType)Args[1], ...);
			 *	}
			 *
			 * OR
			 *
			 *	(ICommandGroup Group, object?[] Args) =>
			 *	{
			 *		return ((DeclaringType)Group).Method((ParamType)Args[0], (ParamType)Args[1], ...);
			 *	}
			 */

			var args = Expression.Parameter(typeof(object?[]), "Args");
			var argsCast = method.GetParameters().Select((x, i) =>
			{
				var access = Expression.ArrayAccess(args, Expression.Constant(i));
				return Expression.Convert(access, x.ParameterType);
			});

			// Make sure we're calling the method on something that actually implements it
			var instanceCast = instance is null
				? null
				: Expression.Convert(instance, method.DeclaringType);

			Expression body = Expression.Call(instanceCast, method, argsCast);

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

			var parameters = instance is ParameterExpression instanceParameter
				? new[] { instanceParameter, args }
				: new[] { args };

			var lambda = Expression.Lambda<T>(
				body,
				parameters
			);
			return lambda.Compile();
		}

		internal static IEnumerable<Expression> SelectWritableMembers(
			this Type type,
			ParameterExpression instance,
			Func<MemberExpression, Expression> createExpression)
		{
			Expression Convert(MemberInfo info)
			{
				var instanceCast = Expression.Convert(instance, info.DeclaringType);
				var access = Expression.MakeMemberAccess(instanceCast, info);
				return createExpression(access);
			}

			var (properties, fields) = type.GetWritableMembers();
			var propertyExpressions = properties.Select(Convert);
			var fieldExpressions = fields.Select(Convert);
			return propertyExpressions.Concat(fieldExpressions);
		}
	}
}