using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	public class NamedArgumentsTypeReader<T> : NamedArgumentsTypeReaderBase<T>
		where T : new()
	{
		private readonly Lazy<IReadOnlyDictionary<string, IImmutableParameter>> _Parameters;
		private readonly Action<T, string, object?> _Setter;

		protected override IReadOnlyDictionary<string, IImmutableParameter> Parameters => _Parameters.Value;

		public NamedArgumentsTypeReader()
		{
			_Parameters = new(() =>
			{
				return NamedArgumentsUtils
					.CreateParametersForType(typeof(T))
					.ToImmutableDictionary(x => x.ParameterName, StringComparer.OrdinalIgnoreCase);
			});
			_Setter = ReflectionUtils.CreateDelegate(Setter, "setter");
		}

		protected override void Setter(T instance, string property, object? value)
			=> _Setter.Invoke(instance, property, value);

		private static Action<T, string, object?> Setter()
		{
			/*
			 *	(T Instance, string Name, object Value) =>
			 *	{
			 *		if (Name == "MemberA")
			 *		{
			 *			Instance.MemberA = (A)Value;
			 *			return;
			 *		}
			 *		if (Name == "MemberB")
			 *		{
			 *			Instance.MemberB = (B)Value;
			 *			return;
			 *		}
			 *		if (Name == "MemberC")
			 *		{
			 *			Instance.MemberC = (C)Value;
			 *			return;
			 *		}
			 *	}
			 */

			var instance = Expression.Parameter(typeof(T), "Instance");
			var name = Expression.Parameter(typeof(string), "Name");
			var value = Expression.Parameter(typeof(object), "Value");

			var setters = typeof(T).CreateExpressionsForWritableMembers<Expression>(instance, x =>
			{
				// If Name == memberInfo.Name
				var memberName = Expression.Constant(x.Member.Name);
				var isMember = Expression.Equal(memberName, name);

				// Then set member and return
				var valueCast = Expression.Convert(value, x.Type);
				var assign = Expression.Assign(x, valueCast);
				var @null = Expression.Constant(null);
				var body = Expression.Block(assign, @null);

				return Expression.IfThen(isMember, body);
			});
			var body = Expression.Block(setters);

			var lambda = Expression.Lambda<Action<T, string, object?>>(
				body,
				instance,
				name,
				value
			);
			return lambda.Compile();
		}
	}
}