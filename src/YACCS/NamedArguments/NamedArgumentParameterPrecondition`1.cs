using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public class NamedArgumentParameterPrecondition<T> : NamedArgumentParameterPreconditionBase<T>
	{
		private readonly Func<T, string, object> _Getter;
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;

		protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentParameterPrecondition()
		{
			_Getter = ReflectionUtils.CreateDelegate(Getter, "getter");
			_Parameters = new Lazy<IReadOnlyDictionary<string, IImmutableParameter>>(() =>
			{
				return NamedArgumentUtils
					.CreateParametersForType(typeof(T))
					.ToDictionary(x => x.OriginalParameterName, StringComparer.OrdinalIgnoreCase);
			});
		}

		protected override object? Getter(T instance, string property)
			=> _Getter.Invoke(instance, property);

		private static Func<T, string, object> Getter()
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

			var instance = Expression.Parameter(typeof(T), "Instance");
			var name = Expression.Parameter(typeof(string), "Name");

			var returnLabel = Expression.Label(typeof(object));
			var getters = typeof(T).CreateExpressionsForWritableMembers<Expression>(instance, x =>
			{
				// If Name == memberInfo.Name
				var memberName = Expression.Constant(x.Member.Name);
				var isMember = Expression.Equal(memberName, name);

				// Then get member and return
				var cast = Expression.Convert(x, typeof(object));
				var @return = Expression.Return(returnLabel, cast);

				return Expression.IfThen(isMember, @return);
			});
			var body = Expression.Block(
				getters
				.Append(Expression.Throw(Expression.Constant(new ArgumentException("Invalid member name.", "Name"))))
				.Append(Expression.Label(returnLabel, Expression.Constant(null)))
			);

			var lambda = Expression.Lambda<Func<T, string, object>>(
				body,
				instance,
				name
			);
			return lambda.Compile();
		}
	}
}