using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public class NamedArgumentsParameterPrecondition<T>
		: NamedArgumentsParameterPreconditionBase<T>
	{
		private static readonly object NotFound = new();
		private readonly Func<T, string, object> _Getter;
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;

		protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentsParameterPrecondition()
		{
			_Getter = ReflectionUtils.CreateDelegate(Getter, "getter");
			_Parameters = new(() => typeof(T).CreateParamDict(x => x.OriginalParameterName));
		}

		protected override bool TryGetProperty(T instance, string property, out object? value)
			=> (value = _Getter.Invoke(instance, property)) != NotFound;

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
			 *		return NotFound;
			 *	}
			 */

			var instance = Expression.Parameter(typeof(T), "Instance");
			var name = Expression.Parameter(typeof(string), "Name");

			var objectLabel = Expression.Label(typeof(object));
			var getters = typeof(T).CreateExpressionsForWritableMembers<Expression>(instance, x =>
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

			var lambda = Expression.Lambda<Func<T, string, object>>(
				body,
				instance,
				name
			);
			return lambda.Compile();
		}
	}
}