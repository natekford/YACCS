using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.NamedArguments
{
	public class NamedArgumentParameterPrecondition<T> : ParameterPrecondition<IContext, T>
	{
		private readonly Lazy<Func<T, string, object>> _Getter;
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;

		protected virtual IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentParameterPrecondition()
		{
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return NamedArgumentUtils
					.CreateParametersForType(typeof(T))
					.ToDictionary(x => x.ParameterName, StringComparer.OrdinalIgnoreCase);
			});
			_Getter = ReflectionUtils.CreateDelegate(CreateGetterDelegate,
				"getter delegate");
		}

		public override async Task<IResult> CheckAsync(
			IImmutableParameter parameter,
			IContext context,
			[MaybeNull] T value)
		{
			foreach (var kvp in Parameters)
			{
				var (id, member) = kvp;
				foreach (var precondition in member.Preconditions)
				{
					var memberValue = Getter(value, id);
					var result = await precondition.CheckAsync(member, context, memberValue).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						return result;
					}
				}
			}
			return SuccessResult.Instance.Sync;
		}

		protected virtual object? Getter(T instance, string property)
			=> _Getter.Value.Invoke(instance, property);

		private static Func<T, string, object> CreateGetterDelegate()
		{
			/*
			 *	(T Instance, string Name) =>
			 *	{
			 *		if (Name == "MemberA")
			 *		{
			 *			return Instance.MemberA;
			 *		}
			 *		if (Name == "MemberB")
			 *		{
			 *			return Instance.MemberB;
			 *		}
			 *		if (Name == "MemberC")
			 *		{
			 *			return Instance.MemberC;
			 *		}
			 *
			 *		throw new ArgumentException();
			 *		return null;
			 *	}
			 */

			var instanceExpr = Expression.Parameter(typeof(T), "Instance");
			var nameExpr = Expression.Parameter(typeof(string), "Name");
			var returnLabel = Expression.Label(typeof(object));

			var (properties, fields) = typeof(T).GetWritableMembers();

			Expression CreateExpression(MemberInfo member)
			{
				var accessExpr = Expression.MakeMemberAccess(instanceExpr, member);
				var castExpr = Expression.Convert(accessExpr, typeof(object));

				var returnExpr = Expression.Return(returnLabel, castExpr);
				var memberNameExpr = Expression.Constant(member.Name);
				var isMemberExpr = Expression.Equal(memberNameExpr, nameExpr);
				return Expression.IfThen(isMemberExpr, returnExpr);
			}

			var propertyExprs = properties.Select(CreateExpression);
			var fieldExprs = fields.Select(CreateExpression);
			var allGetExpr = Expression.Block(
				propertyExprs
				.Concat(fieldExprs)
				.Append(Expression.Throw(Expression.Constant(new ArgumentException("Invalid member name.", "Name"))))
				.Append(Expression.Label(returnLabel, Expression.Constant(null)))
			);

			var lambda = Expression.Lambda<Func<T, string, object>>(
				allGetExpr,
				instanceExpr,
				nameExpr
			);
			return lambda.Compile();
		}
	}
}