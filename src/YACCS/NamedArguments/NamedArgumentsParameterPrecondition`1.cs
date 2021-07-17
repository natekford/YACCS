using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public class NamedArgumentsParameterPrecondition<TValue>
		: NamedArgumentsParameterPreconditionBase<TValue>
	{
		private readonly Func<TValue, string, object> _Getter;
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;

		protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentsParameterPrecondition()
		{
			_Getter = ReflectionUtils.CreateDelegate(Getter, "getter");
			_Parameters = new(() =>
			{
				return NamedArgumentsUtils
					.CreateParametersForType(typeof(TValue))
					.ToImmutableDictionary(x => x.OriginalParameterName, StringComparer.OrdinalIgnoreCase);
			});
		}

		protected override object? Getter(TValue instance, string property)
		{
			try
			{
				return _Getter.Invoke(instance, property);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					$"Unable to find the specified member '{property}'.", e);
			}
		}

		private static Func<TValue, string, object> Getter()
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
			 *		throw new InvalidOperationException();
			 *		return null;
			 *	}
			 */

			var instance = Expression.Parameter(typeof(TValue), "Instance");
			var name = Expression.Parameter(typeof(string), "Name");

			var objectLabel = Expression.Label(typeof(object));
			var getters = typeof(TValue).CreateExpressionsForWritableMembers<Expression>(instance, x =>
			{
				// If Name == memberInfo.Name
				var memberName = Expression.Constant(x.Member.Name);
				var isMember = Expression.Equal(memberName, name);

				// Then get member and return
				var cast = Expression.Convert(x, typeof(object));
				var @return = Expression.Return(objectLabel, cast);

				return Expression.IfThen(isMember, @return);
			});
			var exception = Expression.Constant(new InvalidOperationException("Unable to find the specified member name."));
			var @throw = Expression.Throw(exception);
			var @null = Expression.Constant(null);
			var returnLabel = Expression.Label(objectLabel, @null);
			var body = Expression.Block(getters.Append(@throw).Append(returnLabel));

			var lambda = Expression.Lambda<Func<TValue, string, object>>(
				body,
				instance,
				name
			);
			return lambda.Compile();
		}
	}
}