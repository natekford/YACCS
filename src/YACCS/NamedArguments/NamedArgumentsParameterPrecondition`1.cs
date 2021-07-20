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
		private static readonly object NotFound = new();
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

		protected override bool TryGetValue(TValue instance, string property, out object? value)
		{
			value = _Getter.Invoke(instance, property);
			if (value == NotFound)
			{
				value = null;
				return false;
			}
			return true;
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
			 *		return NotFound;
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
			var notFound = Expression.Constant(NotFound);
			var returnLabel = Expression.Label(objectLabel, notFound);
			var body = Expression.Block(getters.Append(notFound).Append(returnLabel));

			var lambda = Expression.Lambda<Func<TValue, string, object>>(
				body,
				instance,
				name
			);
			return lambda.Compile();
		}
	}
}